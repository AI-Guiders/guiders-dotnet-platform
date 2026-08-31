#nullable enable

using AIGuiders.Platform.CommandPlane.ArgSuggestions;

namespace AIGuiders.Platform.CommandPlane;

/// <summary>Arg-tail picker completion (GUIDERS-ADR-0012, ADR-0040).</summary>
static class SlashArgCompletion
{
    public static bool ShouldComplete(SlashLineResolver.SlashLineResolution line, CatalogRouteEntry route) =>
        route.ArgTailKind != CommandArgTailKind.None
        && line.IsCatalogMatch
        && (line.IsExactPathMatch
            || line.EndsWithSpaceAfterPath
            || line.HasArgTailContent
            || route.ArgTailKind == CommandArgTailKind.Picker);

    public static IReadOnlyList<SlashCompletionItem> GetSuggestions(
        SlashLineResolver.SlashLineResolution line,
        CatalogRouteEntry route,
        ICommandArgSuggestionBroker? suggestionBroker)
    {
        var partial = line.ArgTail.Trim();
        var items = new List<SlashCompletionItem>();

        var choices = ResolveChoices(line, route, partial, suggestionBroker);
        if (choices.Count > 0)
        {
            items.AddRange(BuildPickerItems(line, route, choices, partial));
        }

        if (partial.Length == 0)
        {
            items.AddRange(SlashConstructorCompletion.BuildEntryItems(line, route));
        }
        else if (route.ResolvedConstructors.Count > 0)
        {
            items.AddRange(FilterConstructorEntries(line, route, partial));
        }

        return SlashCompletionSort.Order(items);
    }

    static IReadOnlyList<SlashCompletionItem> FilterConstructorEntries(
        SlashLineResolver.SlashLineResolution line,
        CatalogRouteEntry route,
        string partial)
    {
        var all = SlashConstructorCompletion.BuildEntryItems(line, route);
        if (partial.Length == 0)
        {
            return all;
        }

        return all
            .Where(item =>
                (item.StepSegment?.Contains(partial, StringComparison.OrdinalIgnoreCase) ?? false)
                || (item.Help?.Contains(partial, StringComparison.OrdinalIgnoreCase) ?? false)
                || (item.PickValue?.Contains(partial, StringComparison.OrdinalIgnoreCase) ?? false))
            .ToList();
    }

    public static bool HasChoices(
        CatalogRouteEntry route,
        string partial,
        ICommandArgSuggestionBroker? suggestionBroker) =>
        ResolveChoices(null, route, partial, suggestionBroker).Count > 0
        || route.ResolvedConstructors.Count > 0;

    static IReadOnlyList<SlashCompletionItem> BuildPickerItems(
        SlashLineResolver.SlashLineResolution line,
        CatalogRouteEntry route,
        IReadOnlyList<CommandPickerChoice> choices,
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

            if (choice.Kind == CommandPickerChoiceKind.Constructor)
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

    static IReadOnlyList<CommandPickerChoice> ResolveChoices(
        SlashLineResolver.SlashLineResolution? line,
        CatalogRouteEntry route,
        string partial,
        ICommandArgSuggestionBroker? suggestionBroker)
    {
        if (route.ResolvedPickerChoices.Count > 0)
        {
            return route.ResolvedPickerChoices;
        }

        if (route.ArgTailKind != CommandArgTailKind.Picker || suggestionBroker is null)
        {
            return [];
        }

        var suggestionId = CommandArgTailPolicy.ExtractSuggestionId(route.ArgTail);
        if (suggestionId is null)
        {
            return [];
        }

        var canonicalPath = line?.CanonicalPath ?? route.Path;
        return suggestionBroker.GetSuggestions(
            ArgSuggestionRequest.Create(suggestionId, partial, route, canonicalPath));
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

    static bool MatchesPickerChoice(CommandPickerChoice choice, string partial)
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
