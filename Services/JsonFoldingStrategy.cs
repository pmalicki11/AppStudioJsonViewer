using System.Collections.Generic;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;

namespace AppStudioJsonViewer.Services;

/// <summary>
/// Produces collapsible regions for JSON objects (<c>{ }</c>) and arrays
/// (<c>[ ]</c>) that span more than one line. Brace/bracket characters that
/// appear inside string literals are ignored.
/// </summary>
public static class JsonFoldingStrategy
{
    /// <summary>Recomputes foldings for <paramref name="document"/> and applies them.</summary>
    public static void Update(FoldingManager manager, TextDocument document)
    {
        manager.UpdateFoldings(CreateFoldings(document), firstErrorOffset: -1);
    }

    private static IEnumerable<NewFolding> CreateFoldings(TextDocument document)
    {
        var text = document.Text;
        var openers = new Stack<int>();   // offsets of unmatched '{' or '['
        var foldings = new List<NewFolding>();
        var inString = false;

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];

            if (inString)
            {
                if (c == '\\') i++;            // skip the escaped character
                else if (c == '"') inString = false;
                continue;
            }

            switch (c)
            {
                case '"':
                    inString = true;
                    break;
                case '{':
                case '[':
                    openers.Push(i);
                    break;
                case '}':
                case ']':
                    if (openers.Count == 0) break;
                    var start = openers.Pop();
                    // Only worth folding if the region actually covers multiple lines.
                    if (document.GetLineByOffset(start).LineNumber
                        != document.GetLineByOffset(i).LineNumber)
                    {
                        foldings.Add(new NewFolding(start, i + 1));
                    }
                    break;
            }
        }

        // UpdateFoldings requires the list ordered by start offset.
        foldings.Sort((a, b) => a.StartOffset.CompareTo(b.StartOffset));
        return foldings;
    }
}
