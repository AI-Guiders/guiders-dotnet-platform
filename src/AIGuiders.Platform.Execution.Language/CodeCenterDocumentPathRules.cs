using System.IO;

namespace AIGuiders.Platform.Execution.Language;

/// <summary>Extension matching for Code Center language plugins.</summary>
public static class CodeCenterDocumentPathRules
{
    public static bool HasAnyExtension(string documentPathOrId, IReadOnlyList<string> extensions)
    {
        if (extensions.Count == 0)
        {
            return false;
        }

        var path = StripQuery(documentPathOrId);
        foreach (var extension in extensions)
        {
            if (string.IsNullOrWhiteSpace(extension))
            {
                continue;
            }

            var normalized = extension.StartsWith('.') ? extension : "." + extension;
            if (string.Equals(Path.GetExtension(path), normalized, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    static string StripQuery(string documentPathOrId)
    {
        var query = documentPathOrId.IndexOf('?', StringComparison.Ordinal);
        return query < 0 ? documentPathOrId : documentPathOrId[..query];
    }
}
