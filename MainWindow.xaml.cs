using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using AppStudioJsonViewer.Models;
using AppStudioJsonViewer.Services;

namespace AppStudioJsonViewer;

public partial class MainWindow : Window
{
    private readonly AppConfig _config;
    private XxxDefRepository _repository;

    public MainWindow()
    {
        InitializeComponent();
        _config = AppConfig.Load();

        EnvironmentComboBox.ItemsSource = _config.Environments;
        EnvironmentComboBox.SelectedIndex = 0;

        _repository = new XxxDefRepository(_config, _config.Environments[0].ConnectionString);

        StatusText.Text = "Select an environment and click Load.";
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        WindowTheming.UseDarkTitleBar(this);
    }

    private void EnvironmentComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (EnvironmentComboBox.SelectedItem is not EnvironmentEntry env) return;
        _repository = new XxxDefRepository(_config, env.ConnectionString);
        RowsGrid.ItemsSource = null;
        StatusText.Text = "Select an environment and click Load.";
    }

    private async Task LoadRowsAsync()
    {
        try
        {
            LoadingOverlay.Visibility = Visibility.Visible;
            LoadButton.IsEnabled = false;
            StatusText.Text = "Loading…";

            var rows = await _repository.GetLayerRowsAsync();
            RowsGrid.ItemsSource = rows;

            var view = CollectionViewSource.GetDefaultView(rows);
            view.Filter = RowMatchesFilters;

            StatusText.Text = $"{rows.Count} row(s). Click a layer to view its content.";
        }
        catch (Exception ex)
        {
            RowsGrid.ItemsSource = null;
            StatusText.Text = "Failed to load.";
            MessageBox.Show(this,
                $"Could not connect to the selected environment.\n\n{ex.Message}\n\n" +
                "Check the connection string in appsettings.json.",
                "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
            LoadButton.IsEnabled = true;
        }
    }

    private async void LoadButton_Click(object sender, RoutedEventArgs e) => await LoadRowsAsync();

    private void Filter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (RowsGrid.ItemsSource is { } source)
            CollectionViewSource.GetDefaultView(source)?.Refresh();
    }

    private bool RowMatchesFilters(object item)
    {
        if (item is not XxxDefRow row) return false;
        return Contains(row.Key1, Key1Filter?.Text)
            && Contains(row.Key2, Key2Filter?.Text);

        static bool Contains(string value, string? term) =>
            string.IsNullOrWhiteSpace(term)
            || value.Contains(term.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private async void Key1_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Hyperlink { DataContext: XxxDefRow row })
        {
            row.Content = await _repository.GetContentAsync(row);
            var editor = new EditorWindow(_repository, row) { Owner = this };
            editor.ShowDialog();
        }
    }
}
