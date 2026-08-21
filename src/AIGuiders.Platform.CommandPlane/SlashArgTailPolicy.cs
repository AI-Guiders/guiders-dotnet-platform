#nullable enable

namespace AIGuiders.Platform.CommandPlane;

/// <summary>Parse ArgTail wire strings (Forge + CIDE TOML).</summary>
public static class SlashArgTailPolicy
{
    public static SlashArgTailKind Parse(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return SlashArgTailKind.Optional;

        var t = raw.Trim();
        if (t.Equals("none", StringComparison.OrdinalIgnoreCase))
            return SlashArgTailKind.None;
        if (t.Equals("required", StringComparison.OrdinalIgnoreCase))
            return SlashArgTailKind.Required;
        if (t.Equals("optional", StringComparison.OrdinalIgnoreCase))
            return SlashArgTailKind.Optional;
        if (t.StartsWith("picker:", StringComparison.OrdinalIgnoreCase))
            return SlashArgTailKind.Picker;

        return SlashArgTailKind.Optional;
    }

    public static bool ShouldAutoRunOnCommit(SlashArgTailKind kind, bool isExactPath, bool endsWithSpace, bool hasArgTail) =>
        kind switch
        {
            SlashArgTailKind.None => isExactPath,
            SlashArgTailKind.Optional => isExactPath || endsWithSpace || hasArgTail,
            SlashArgTailKind.Required => hasArgTail,
            SlashArgTailKind.Picker => endsWithSpace || hasArgTail,
            _ => false,
        };

    public static bool InsertsTrailingSpaceOnCommit(SlashArgTailKind kind) =>
        kind is not SlashArgTailKind.None;
}
