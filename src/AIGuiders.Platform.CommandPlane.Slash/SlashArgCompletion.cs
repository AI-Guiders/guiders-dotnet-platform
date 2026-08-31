#nullable enable

namespace AIGuiders.Platform.CommandPlane;

/// <summary>Arg-tail picker completion (GUIDERS-ADR-0012).</summary>
static class SlashArgCompletion
{
    public static bool ShouldComplete(SlashLineResolver.SlashLineResolution line, SlashRouteEntry route) =>
        route.ArgTailKind != SlashArgTailKind.None
        && line.IsCatalogMatch
        && (line.IsExactPathMatch
            || line.EndsWithSpaceAfterPath
            || line.HasArgTailContent
            || route.ArgTailKind == SlashArgTailKind.Picker);

    public static IReadOnlyList<SlashCompletionItem> GetSuggestions(
        SlashLineResolver.SlashLineResolution line,
        SlashRouteEntry route,
        ISlashPickerChoiceSource? pickerSource)
    {
        var partial = line.ArgTail.Trim();
        var items = new List<SlashCompletionItem>();

        var choices = ResolveChoices(route, partial, pickerSource);
        if (choices.Count > 0)
        {
            items.AddRange(BuildPickerItems(line, route, choices, partial));
        }

        if (partial.Length == 0)
        {
            items.AddRange(SlashConstructorCompletion.BuildEntryItems(line, route));
        }

        return SlashCompletionSort.Order(items);
    }

    public static bool HasChoices(
        SlashRouteEntry route,
        string partial,
        ISlashPickerChoiceSource? pickerSource) =>
        ResolveChoices(route, partial, pickerSource).Count > 0
        || (partial.Length == 0 && route.ResolvedConstructors.Count > 0);

    static IReadOnlyList<SlashCompletionItem> BuildPickerItems(
        SlashLineResolver.SlashLineResolution line,
        SlashRouteEntry route,
        IReadOnlyList<SlashPickerChoice> choices,
        string partial)
    {
        var canonicalPath = "/" + line.CanonicalPath.TrimStart('/');
        var buckets = new Dictionary<string, SlashCompletionItem>(StringComparer.OrdinalIgnoreCase);
        foreach (var choice in choices)
        {
            if (!MatchesPickerChoice(choice, partial))
            {
                continue;
            }

            if (choice.Kind == SlashPickerChoiceKind.Constructor)
            {
                var constructorLabel = choice.Label ?? choice.Value;
                buckets[constructorLabel] = new SlashCompletionItem(
                    canonicalPath + " ",
                    line.CanonicalPath,
                    choice.Hint ?? constructorLabel,
                    route.Group,
                    constructorLabel,
                    SlashCompletionItemKind.ConstructorEntry,
                    choice.Value);
                continue;
            }

            var value = choice.Value.Trim();
            if (value.Length == 0)
            {
                continue;
            }

            var label = string.IsNullOrWhiteSpace(choice.Label) ? value : choice.Label.Trim();
            var insert = canonicalPath + " " + value;
            var help = string.IsNullOrWhiteSpace(choice.Hint) ? label : choice.Hint.Trim();
            AddPickerSuggestion(buckets, label, insert, canonicalPath, help, route.Group, value);
        }

        return SlashCompletionSort.Order(buckets.Values);
    }

    static IReadOnlyList<SlashPickerChoice> ResolveChoices(
        SlashRouteEntry route,
        string partial,
        ISlashPickerChoiceSource? pickerSource)
    {
        if (route.ResolvedPickerChoices.Count > 0)
        {
            return route.ResolvedPickerChoices;
        }

        if (route.ArgTailKind != SlashArgTailKind.Picker || pickerSource is null)
        {
            return [];
        }

        var pickerId = SlashArgTailPolicy.ExtractPickerId(route.ArgTail);
        return pickerId is null ? [] : pickerSource.GetChoices(pickerId, partial);
    }

    static void AddPickerSuggestion(
        Dictionary<string, SlashCompletionItem> buckets,
        string listTitle,
        string insert,
        string slashPath,
        string help,
        string? group,
        string value)
    {
        if (!buckets.TryGetValue(listTitle, out var existing)
            || slashPath.Length >= existing.SlashPath.Length)
        {
            buckets[listTitle] = new SlashCompletionItem(
                insert,
                slashPath,
                help,
                group,
                listTitle,
                SlashCompletionItemKind.Picker,
                value);
        }
    }

    static bool MatchesPickerChoice(SlashPickerChoice choice, string partial)
    {
        if (partial.Length == 0)
        {
            return true;
        }

        var value = choice.Value ?? "";
        var label = choice.Label ?? "";
        var hint = choice.Hint ?? "";
        return value.StartsWith(partial, StringComparison.OrdinalIgnoreCase)
               || label.Contains(partial, StringComparison.OrdinalIgnoreCase)
               || hint.Contains(partial, StringComparison.OrdinalIgnoreCase);
    }
}
