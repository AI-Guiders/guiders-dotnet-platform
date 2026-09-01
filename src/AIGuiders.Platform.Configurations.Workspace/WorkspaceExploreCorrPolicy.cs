#nullable enable

using AIGuiders.Platform.Paths;

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

        if (settings is null)
            return Mode.Full;

        var rel = PathBoundary.ToLogical(workspaceRoot, absoluteFilePath);
        if (rel is null)
            return ParseMode(settings.Default, Mode.Full);

        var defaultMode = ParseMode(settings.Default, Mode.Full);
        var matched = MatchRule(settings.Rules, rel.Value);
        return matched is not null ? ParseMode(matched, defaultMode) : defaultMode;
    }

    static string? MatchRule(IReadOnlyList<WorkspaceExploreCorrRule>? rules, LogicalPath rel)
    {
        if (rules is not { Count: > 0 })
            return null;

        string? bestPath = null;
        var bestLen = -1;
        string? bestMode = null;

        foreach (var row in rules)
        {
            var key = new LogicalPath(row.Path ?? "");
            if (key.IsEmpty)
                continue;

            if (key.Value == "*")
            {
                if (bestPath is null)
                {
                    bestPath = key.Value;
                    bestLen = 0;
                    bestMode = row.Mode;
                }

                continue;
            }

            if (!rel.StartsWith(key))
                continue;
            if (key.Value.Length <= bestLen)
                continue;

            bestPath = key.Value;
            bestLen = key.Value.Length;
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
}
