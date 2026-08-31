#nullable enable

namespace AIGuiders.Platform.CommandPlane;

/// <summary>Parse ArgTail wire strings (Forge + CIDE TOML).</summary>
public static class SlashArgTailPolicy
{
    public const string ImplicitSelection = "implicit:selection";
    public const string ImplicitLineRange = "implicit:line_range";

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
        if (t.Equals(ImplicitSelection, StringComparison.OrdinalIgnoreCase))
            return SlashArgTailKind.ImplicitSelection;
        if (t.Equals(ImplicitLineRange, StringComparison.OrdinalIgnoreCase))
            return SlashArgTailKind.ImplicitLineRange;
        if (t.StartsWith("picker+constructor:", StringComparison.OrdinalIgnoreCase)
            || t.StartsWith("picker:", StringComparison.OrdinalIgnoreCase))
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
            SlashArgTailKind.ImplicitSelection => isExactPath,
            SlashArgTailKind.ImplicitLineRange => isExactPath || hasArgTail,
            _ => false,
        };

    public static bool InsertsTrailingSpaceOnCommit(SlashArgTailKind kind) =>
        kind is SlashArgTailKind.None;

    public static string? ExtractPickerId(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var text = raw.Trim();
        if (text.StartsWith("picker+constructor:", StringComparison.OrdinalIgnoreCase))
        {
            text = text["picker+constructor:".Length..].Trim();
        }
        else if (!text.StartsWith("picker:", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }
        else
        {
            text = text["picker:".Length..].Trim();
        }

        var plus = text.IndexOf('+');
        var id = plus < 0 ? text : text[..plus].Trim();
        return id.Length == 0 ? null : id;
    }

    public static bool IsCompositePickerConstructor(string? raw) =>
        !string.IsNullOrWhiteSpace(raw)
        && raw.Trim().StartsWith("picker+constructor:", StringComparison.OrdinalIgnoreCase);

    public static IReadOnlyList<string> ExtractCompositeConstructorIds(string? raw)
    {
        if (!IsCompositePickerConstructor(raw))
        {
            return [];
        }

        var text = raw!.Trim()["picker+constructor:".Length..].Trim();
        var parts = text.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length <= 1 ? [] : parts[1..];
    }

    public static bool IsStaticEnumPicker(string? raw)
    {
        var id = ExtractPickerId(raw);
        return id is not null
               && id.StartsWith("enum", StringComparison.OrdinalIgnoreCase);
    }
}
