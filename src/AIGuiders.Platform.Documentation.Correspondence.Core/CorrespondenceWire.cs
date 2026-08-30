#nullable enable

using System.Text.RegularExpressions;

namespace AIGuiders.Platform.Documentation.Correspondence;

public static partial class CorrespondenceWire
{
    public static string Build(string file, int? lineStart, int? lineEnd, string? member)
    {
        var parts = new List<string> { $"F:{CorrespondencePaths.NormalizePath(file)}" };
        if (member is { Length: > 0 })
            parts.Add($"M:{member}");
        else if (lineStart is int ls)
        {
            parts.Add($"L:{ls}");
            if (lineEnd is int le && le != ls)
                parts.Add($"L2:{le}");
        }

        return "[" + string.Join("; ", parts) + "]";
    }

    public static bool TryParseBracket(
        string bracket,
        out string file,
        out int? lineStart,
        out int? lineEnd,
        out string? member)
    {
        file = "";
        lineStart = null;
        lineEnd = null;
        member = null;
        var raw = bracket.Trim().Trim('[', ']');
        raw = BracketKeySepRegex().Replace(raw, "; ");
        foreach (var part in raw.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (part.StartsWith("F:", StringComparison.OrdinalIgnoreCase))
                file = CorrespondencePaths.NormalizePath(part[2..]);
            else if (part.StartsWith("M:", StringComparison.OrdinalIgnoreCase))
                member = part[2..].Trim();
            else if (part.StartsWith("L:", StringComparison.OrdinalIgnoreCase)
                     && int.TryParse(part[2..].Trim(), out var ln))
            {
                lineStart = ln;
                lineEnd ??= ln;
            }
        }

        return file.Length > 0;
    }

    [GeneratedRegex(@"(?<=\S)\s+(?=[FMLSK]:)", RegexOptions.CultureInvariant)]
    private static partial Regex BracketKeySepRegex();
}
