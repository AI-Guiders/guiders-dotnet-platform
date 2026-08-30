#nullable enable

namespace AIGuiders.Platform.Configurations.Workspace;

/// <summary>Per-path Explore correspondence gate tier from workspace.toml <c>[workspace.explore_corr]</c>.</summary>
public static class WorkspaceExploreCorrPolicy
{
    public enum Mode
    {
        Full,
        Card,
        Off,
    }

    public static Mode ResolveMode(
        string absoluteFilePath,
        string workspaceRoot,
        WorkspaceExploreCorrSettings? settings)
    {
        if (string.IsNullOrWhiteSpace(absoluteFilePath))
            return Mode.Full;

        string abs;
        try
        {
            abs = Path.GetFullPath(absoluteFilePath.Trim());
        }
        catch
        {
            return Mode.Full;
        }

        if (settings is null)
            return Mode.Full;

        var defaultMode = ParseMode(settings.Default, Mode.Full);
        var rel = TryRel(workspaceRoot, abs);
        if (rel is null)
            return defaultMode;

        var matched = MatchRule(settings.Rules, rel);
        return matched is not null ? ParseMode(matched, defaultMode) : defaultMode;
    }

    static string? MatchRule(IReadOnlyList<WorkspaceExploreCorrRule>? rules, string rel)
    {
        if (rules is not { Count: > 0 })
            return null;

        string? bestPath = null;
        var bestLen = -1;
        string? bestMode = null;

        foreach (var row in rules)
        {
            var key = NormalizePath(row.Path ?? "");
            if (key.Length == 0)
                continue;

            if (key == "*")
            {
                if (bestPath is null)
                {
                    bestPath = key;
                    bestLen = 0;
                    bestMode = row.Mode;
                }

                continue;
            }

            if (!rel.StartsWith(key, StringComparison.OrdinalIgnoreCase))
                continue;
            if (key.Length <= bestLen)
                continue;

            bestPath = key;
            bestLen = key.Length;
            bestMode = row.Mode;
        }

        return bestMode;
    }

    public static Mode ParseMode(string? raw, Mode fallback)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return fallback;
        var s = raw.Trim();
        if (s.Equals("off", StringComparison.OrdinalIgnoreCase)
            || s.Equals("none", StringComparison.OrdinalIgnoreCase))
            return Mode.Off;
        if (s.Equals("card", StringComparison.OrdinalIgnoreCase)
            || s.Equals("create", StringComparison.OrdinalIgnoreCase))
            return Mode.Card;
        if (s.Equals("full", StringComparison.OrdinalIgnoreCase))
            return Mode.Full;
        return fallback;
    }

    static string? TryRel(string workspaceRoot, string abs)
    {
        var root = Path.GetFullPath(workspaceRoot)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var full = Path.GetFullPath(abs);
        if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase) || full.Length <= root.Length)
            return null;
        return full[(root.Length + 1)..].Replace('\\', '/');
    }

    static string NormalizePath(string raw) =>
        raw.Replace('\\', '/').Trim().TrimStart('/').TrimEnd('/');
}
