using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Search;
using AppStudioJsonViewer.Models;
using AppStudioJsonViewer.Services;

namespace AppStudioJsonViewer;

/// <summary>
/// Shows a single row's Content as prettified JSON, lets the user edit it, and
/// saves it back to Ice.XXXDef minified (no indents / line breaks).
/// </summary>
public partial class EditorWindow : Window
{
    private readonly XxxDefRepository _repository;
    private readonly XxxDefRow _row;
    private readonly FoldingManager _foldingManager;

    public EditorWindow(XxxDefRepository repository, XxxDefRow row)
    {
        InitializeComponent();
        _repository = repository;
        _row = row;

        Editor.SyntaxHighlighting = JsonHighlighting.Definition;

        // Ctrl+F search panel (find, highlight matches, next/prev) and {}/[] folding.
        var searchPanel = SearchPanel.Install(Editor);
        searchPanel.MarkerBrush = new SolidColorBrush(Color.FromRgb(0x09, 0x47, 0x71));
        _foldingManager = FoldingManager.Install(Editor.TextArea);
        StyleFoldingMarginForDarkTheme();

        // Show the stored Content prettified; fall back to raw text if it isn't valid JSON.
        try
        {
            Editor.Text = string.IsNullOrWhiteSpace(row.Content)
                ? ""
                : JsonFormatter.Prettify(row.Content);
            StatusText.Text = "Loaded.";
        }
        catch (Exception)
        {
            Editor.Text = row.Content;
            StatusText.Text = "⚠ Content is not valid JSON — showing raw text.";
        }

        UpdateFoldings();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        WindowTheming.UseDarkTitleBar(this);
    }

    private void UpdateFoldings() =>
        JsonFoldingStrategy.Update(_foldingManager, Editor.Document);

    /// <summary>Makes the fold expand/collapse markers legible on the dark editor.</summary>
    private void StyleFoldingMarginForDarkTheme()
    {
        var marker = (Brush)new BrushConverter().ConvertFromString("#858585")!;
        var background = (Brush)new BrushConverter().ConvertFromString("#1E1E1E")!;
        foreach (var margin in Editor.TextArea.LeftMargins)
        {
            if (margin is not FoldingMargin fm) continue;
            fm.FoldingMarkerBrush = marker;
            fm.FoldingMarkerBackgroundBrush = background;
            fm.SelectedFoldingMarkerBrush = Brushes.White;
            fm.SelectedFoldingMarkerBackgroundBrush = background;
        }
    }

    private void ReformatButton_Click(object sender, RoutedEventArgs e)
    {
        var error = JsonFormatter.Validate(Editor.Text);
        if (error is not null)
        {
            StatusText.Text = "⚠ Invalid JSON — cannot reformat.";
            MessageBox.Show(this, error, "Invalid JSON",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        Editor.Text = JsonFormatter.Prettify(Editor.Text);
        UpdateFoldings();
        StatusText.Text = "Reformatted.";
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var error = JsonFormatter.Validate(Editor.Text);
        if (error is not null)
        {
            StatusText.Text = "⚠ Invalid JSON — not saved.";
            MessageBox.Show(this,
                $"The JSON is not valid, so it was not saved:\n\n{error}",
                "Invalid JSON", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var confirm = MessageBox.Show(this,
            $"Save changes to Content for Key1 = {_row.Key1}?\n\n" +
            "It will be stored minified (no indents or line breaks).",
            "Confirm save", MessageBoxButton.OKCancel, MessageBoxImage.Question);
        if (confirm != MessageBoxResult.OK)
            return;

        try
        {
            var minified = JsonFormatter.Minify(Editor.Text);
            var affected = _repository.UpdateContent(_row, minified);
            if (affected == 1)
            {
                _row.Content = minified;
                StatusText.Text = "Saved.";
                MessageBox.Show(this, "Saved successfully.", "Saved",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                StatusText.Text = $"⚠ Unexpected rows affected: {affected}.";
                MessageBox.Show(this,
                    $"Expected to update exactly 1 row but updated {affected}. " +
                    "No changes were assumed — please re-check the row key.",
                    "Unexpected result", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            StatusText.Text = "Save failed.";
            MessageBox.Show(this, $"Could not save:\n\n{ex.Message}", "Database error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
