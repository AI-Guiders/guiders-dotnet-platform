#nullable enable

using System.Text.RegularExpressions;
using AIGuiders.Platform.Paths;

namespace AIGuiders.Platform.Documentation.Correspondence;

public static partial class CorrespondencePaths
{
    public static string NormalizePath(string raw) => LogicalPath.Normalize(raw);



    public static string NormalizeDoc(string raw) => new LogicalPath(raw).AsDocPath().Value;



    public static string? TryRel(string root, string abs) => PathBoundary.ToLogical(root, abs)?.Value;



    public static bool PathsMatch(string candidatePath, string anchorRel, string anchorFileName) =>

        new LogicalPath(candidatePath).MatchesAnchor(new LogicalPath(anchorRel), anchorFileName);



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

