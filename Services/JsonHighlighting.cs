using System.Reflection;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace AppStudioJsonViewer.Services;

/// <summary>Loads the embedded Json.xshd syntax-highlighting definition.</summary>
public static class JsonHighlighting
{
    private static IHighlightingDefinition? _definition;

    public static IHighlightingDefinition Definition =>
        _definition ??= Load();

    private static IHighlightingDefinition Load()
    {
        var asm = Assembly.GetExecutingAssembly();
        // Embedded resource names use dot-separated paths: <RootNamespace>.Resources.Json.xshd
        var resourceName = asm.GetManifestResourceNames()
            .Single(n => n.EndsWith("Json.xshd", StringComparison.OrdinalIgnoreCase));

        using var stream = asm.GetManifestResourceStream(resourceName)
                           ?? throw new InvalidOperationException("Json.xshd resource not found.");
        using var xmlReader = new XmlTextReader(stream);
        return HighlightingLoader.Load(xmlReader, HighlightingManager.Instance);
    }
}
