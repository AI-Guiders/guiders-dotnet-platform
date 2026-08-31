#nullable enable

namespace AIGuiders.Platform.IntermediateRepresentation.Command;

/// <summary>Parse ArgTail wire strings (Forge + CIDE TOML).</summary>
public static class CommandArgTailPolicy
{
    public const string ImplicitSelection = "implicit:selection";
    public const string ImplicitLineRange = "implicit:line_range";

    public static CommandArgTailKind Parse(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return CommandArgTailKind.Optional;

        var t = raw.Trim();
        if (t.Equals("none", StringComparison.OrdinalIgnoreCase))
            return CommandArgTailKind.None;
        if (t.Equals("required", StringComparison.OrdinalIgnoreCase))
            return CommandArgTailKind.Required;
        if (t.Equals("optional", StringComparison.OrdinalIgnoreCase))
            return CommandArgTailKind.Optional;
        if (t.Equals(ImplicitSelection, StringComparison.OrdinalIgnoreCase))
            return CommandArgTailKind.ImplicitSelection;
        if (t.Equals(ImplicitLineRange, StringComparison.OrdinalIgnoreCase))
            return CommandArgTailKind.ImplicitLineRange;
        if (t.StartsWith("suggest:", StringComparison.OrdinalIgnoreCase)
            || t.StartsWith("picker+constructor:", StringComparison.OrdinalIgnoreCase)
            || t.StartsWith("picker:", StringComparison.OrdinalIgnoreCase))
            return CommandArgTailKind.Picker;

        return CommandArgTailKind.Optional;
    }

    public static bool ShouldAutoRunOnCommit(CommandArgTailKind kind, bool isExactPath, bool endsWithSpace, bool hasArgTail) =>
        kind switch
        {
            CommandArgTailKind.None => isExactPath,
            CommandArgTailKind.Optional => isExactPath || endsWithSpace || hasArgTail,
            CommandArgTailKind.Required => hasArgTail,
            CommandArgTailKind.Picker => endsWithSpace || hasArgTail,
            CommandArgTailKind.ImplicitSelection => isExactPath,
            CommandArgTailKind.ImplicitLineRange => isExactPath || hasArgTail,
            _ => false,
        };

    public static bool InsertsTrailingSpaceOnCommit(CommandArgTailKind kind) =>
        kind is CommandArgTailKind.None;

    public static string? ExtractPickerId(string? raw) => ExtractSuggestionId(raw);

    public static string? ExtractSuggestionId(string? raw)
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
        else if (text.StartsWith("suggest:", StringComparison.OrdinalIgnoreCase))
        {
            text = text["suggest:".Length..].Trim();
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
