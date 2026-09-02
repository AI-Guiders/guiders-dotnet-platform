#nullable enable

using AIGuiders.Platform.Authoring.Command.Catalog;

namespace AIGuiders.Platform.Execution.CommandPlane;

/// <summary>Enrich path completion rows with catalog phrase-slot metadata (GUIDERS-ADR-0054).</summary>
public static class PhraseSlotCompletion
{
    public static ArgCompletionItem Enrich(
        ArgCompletionItem item,
        CatalogPhraseSlotIndex? phraseSlots,
        string typedBody,
        string routeCommandId)
    {
        if (phraseSlots is null)
        {
            return item;
        }

        if (!phraseSlots.TryResolveCommand(typedBody, routeCommandId, out var command))
        {
            return item;
        }

        var activeSlot = command.ResolveActiveSlot(typedBody);
        return item with
        {
            CommandId = routeCommandId,
            ActiveSlot = activeSlot,
            SlotLabel = activeSlot is null ? null : command.GetSlotLabel(activeSlot),
        };
    }
}
