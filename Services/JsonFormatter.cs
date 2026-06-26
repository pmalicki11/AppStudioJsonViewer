using System.Text.Json;
using System.Text.Json.Nodes;

namespace AppStudioJsonViewer.Services;

/// <summary>Prettify / minify helpers built on System.Text.Json.</summary>
public static class JsonFormatter
{
    private static readonly JsonSerializerOptions Indented = new()
    {
        WriteIndented = true,
    };

    private static readonly JsonSerializerOptions Compact = new()
    {
        WriteIndented = false,
    };

    /// <summary>Parses then re-serializes with indentation. Throws on invalid JSON.</summary>
    public static string Prettify(string json)
    {
        var node = JsonNode.Parse(json, documentOptions: new JsonDocumentOptions
        {
            CommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        });
        return node?.ToJsonString(Indented) ?? "";
    }

    /// <summary>Parses then re-serializes with no indentation or line breaks.</summary>
    public static string Minify(string json)
    {
        var node = JsonNode.Parse(json, documentOptions: new JsonDocumentOptions
        {
            CommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        });
        return node?.ToJsonString(Compact) ?? "";
    }

    /// <summary>Returns null if valid, otherwise the parser error message.</summary>
    public static string? Validate(string json)
    {
        try
        {
            JsonNode.Parse(json, documentOptions: new JsonDocumentOptions
            {
                CommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
            });
            return null;
        }
        catch (JsonException ex)
        {
            return ex.Message;
        }
    }
}
