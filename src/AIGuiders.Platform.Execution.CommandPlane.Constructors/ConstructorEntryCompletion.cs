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
            var hint = binding.Hint is not null && Microsoft.FSharp.Core.FSharpOption<string>.get_IsSome(binding.Hint)
                ? binding.Hint.Value
                : binding.Label;
            var group = route.Group is not null && Microsoft.FSharp.Core.FSharpOption<string>.get_IsSome(route.Group)
                ? route.Group.Value
                : null;
            items.Add(new ArgCompletionItem(
                commandPath + " ",
                commandPath,
                hint,
                group,
                binding.Label,
                ArgCompletionItemKind.ConstructorEntry,
                binding.ConstructorId));
        }

        return items;
    }
}
