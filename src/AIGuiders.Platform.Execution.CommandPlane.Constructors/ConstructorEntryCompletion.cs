using AIGuiders.Platform.IntermediateRepresentation.Command;
#nullable enable

namespace AIGuiders.Platform.Execution.CommandPlane;

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

        var commandPath = "/" + canonicalPath.TrimStart('/');
        var items = new List<ArgCompletionItem>();
        foreach (var binding in bindings)
        {
            items.Add(new ArgCompletionItem(
                commandPath + " ",
                commandPath,
                binding.Hint ?? binding.Label,
                route.Group,
                binding.Label,
                ArgCompletionItemKind.ConstructorEntry,
                binding.ConstructorId));
        }

        return items;
    }
}
