using AIGuiders.Platform.IntermediateRepresentation.Command;
#nullable enable

namespace AIGuiders.Platform.CommandPlane;

static class SlashConstructorCompletion
{
    public static IReadOnlyList<SlashCompletionItem> BuildEntryItems(
        SlashLineResolver.SlashLineResolution line,
        CatalogRouteEntry route)
    {
        if (route.ArgConstructors is not { Count: > 0 } bindings)
        {
            return [];
        }

        var canonicalPath = "/" + line.CanonicalPath.TrimStart('/');
        var items = new List<SlashCompletionItem>();
        foreach (var binding in bindings)
        {
            items.Add(new SlashCompletionItem(
                canonicalPath + " ",
                line.CanonicalPath,
                binding.Hint ?? binding.Label,
                route.Group,
                binding.Label,
                SlashCompletionItemKind.ConstructorEntry,
                binding.ConstructorId));
        }

        return items;
    }
}
