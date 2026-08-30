#nullable enable

using System.Text.RegularExpressions;

namespace AIGuiders.Platform.Documentation.Correspondence;

public static partial class CorrespondencePaths
{
    public static string NormalizePath(string raw) => (raw ?? "").Trim().Replace('\\', '/');

    public static string NormalizeDoc(string raw) => NormalizePath(raw).TrimStart('/');

    public static string? TryRel(string root, string abs)
    {
        try
        {
            var r = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var a = Path.GetFullPath(abs);
            if (!a.StartsWith(r, StringComparison.OrdinalIgnoreCase))
                return null;
            return a[r.Length..].TrimStart('\\', '/').Replace('\\', '/');
        }
        catch
        {
            return null;
        }
    }

    public static bool PathsMatch(string candidatePath, string anchorRel, string anchorFileName)
    {
        var c = NormalizePath(candidatePath);
        if (c.Equals(anchorRel, StringComparison.OrdinalIgnoreCase))
            return true;
        if (c.EndsWith('/' + anchorRel, StringComparison.OrdinalIgnoreCase))
            return true;
        return string.Equals(Path.GetFileName(c), anchorFileName, StringComparison.OrdinalIgnoreCase)
            && (anchorRel.EndsWith('/' + anchorFileName, StringComparison.OrdinalIgnoreCase)
                || string.Equals(anchorRel, anchorFileName, StringComparison.OrdinalIgnoreCase));
    }

    public static bool LooksLikeCodePath(string path) =>
        path.Contains('.', StringComparison.Ordinal)
        && (path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".fs", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".vb", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".csx", StringComparison.OrdinalIgnoreCase)
            || path.Contains('/', StringComparison.Ordinal));

    public static string GuessTitle(string path)
    {
        var m = AdrIdRegex().Match(path.Replace('\\', '/'));
        return m.Success ? $"ADR {m.Groups["id"].Value}" : path;
    }

    public static int LineNumberAt(string markdown, int index)
    {
        var limit = Math.Min(Math.Max(index, 0), markdown.Length);
        var line = 1;
        for (var i = 0; i < limit; i++)
        {
            if (markdown[i] == '\n')
                line++;
        }

        return line;
    }

    public static string? ExcerptAt(string markdown, int lineOneBased)
    {
        var lines = markdown.Replace("\r\n", "\n").Split('\n');
        if (lineOneBased < 1 || lineOneBased > lines.Length)
            return null;
        var raw = lines[lineOneBased - 1].Trim();
        return raw.Length <= 96 ? raw : raw[..93] + "…";
    }

    [GeneratedRegex(@"(?:^|/)docs/adr/(?<id>\d{4})-", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex AdrIdRegex();
}
