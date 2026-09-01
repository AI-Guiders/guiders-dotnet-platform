#nullable enable

namespace AIGuiders.Platform.Paths;

/// <summary>Repo/workspace-relative path with forward-slash canonical form (GUIDERS-ADR-0050).</summary>
public readonly record struct LogicalPath
{
    public static LogicalPath Empty { get; } = new("");

    public string Value { get; }

    public LogicalPath(string value) => Value = Normalize(value);

    public bool IsEmpty => Value.Length == 0;

    public static LogicalPath Parse(string raw) => new(raw);

    public static bool TryParse(string? raw, out LogicalPath path)
    {
        if (raw is null)
        {
            path = Empty;
            return false;
        }

        path = new LogicalPath(raw);
        return true;
    }

    /// <summary>Doc/correspondence ids: logical path without a leading slash.</summary>
    public LogicalPath AsDocPath()
    {
        var trimmed = Value.TrimStart('/');
        return trimmed == Value ? this : new LogicalPath(trimmed);
    }

    public LogicalPath Combine(string segment)
    {
        if (string.IsNullOrWhiteSpace(segment))
            return this;

        var left = Value.TrimEnd('/');
        var right = Normalize(segment).TrimStart('/');
        if (left.Length == 0)
            return new LogicalPath(right);
        if (right.Length == 0)
            return this;
        return new LogicalPath($"{left}/{right}");
    }

    public bool Equals(LogicalPath other, StringComparison comparison) =>
        string.Equals(Value, other.Value, comparison);

    public bool StartsWith(LogicalPath prefix, StringComparison comparison = StringComparison.OrdinalIgnoreCase) =>
        prefix.IsEmpty || Value.StartsWith(prefix.Value, comparison);

    public bool MatchesAnchor(LogicalPath anchor, string anchorFileName) =>
        LogicalPathMatching.Matches(Value, anchor.Value, anchorFileName);

    public override string ToString() => Value;

    public static string Normalize(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return "";

        var normalized = raw.Trim().Replace('\\', '/');
        while (normalized.Contains("//", StringComparison.Ordinal))
            normalized = normalized.Replace("//", "/", StringComparison.Ordinal);

        return normalized.TrimStart('.').TrimStart('/').TrimEnd('/');
    }
}
