using AIGuiders.Platform.IntermediateRepresentation.Command;
#nullable enable

namespace AIGuiders.Platform.CommandPlane;

public static class ConstructorEntryCompletion
{
    public static IReadOnlyList<ArgCompletionItem> BuildEntryItems(
        string canonicalPath,
        CatalogRouteEntry route)
    {
        if (route.ArgConstructors is not { Count: > 0 } bindings)
        {
            return [];
        }

        var slashPath = "/" + canonicalPath.TrimStart('/');
        var items = new List<ArgCompletionItem>();
        foreach (var binding in bindings)
        {
            items.Add(new ArgCompletionItem(
                slashPath + " ",
                canonicalPath,
                binding.Hint ?? binding.Label,
                route.Group,
                binding.Label,
                ArgCompletionItemKind.ConstructorEntry,
                binding.ConstructorId));
        }

        return items;
    }
}
