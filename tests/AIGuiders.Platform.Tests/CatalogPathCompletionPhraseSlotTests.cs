#nullable enable

using AIGuiders.Platform.Authoring.Command.Catalog;
using AIGuiders.Platform.Execution.CommandPlane;
using AIGuiders.Platform.IntermediateRepresentation.Command;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class CatalogPathCompletionPhraseSlotTests
{
    [Fact]
    public void Enriches_items_with_active_slot_metadata()
    {
        var phraseSlots = CatalogPhraseSlotIndex.FromEmitted(
        [
            new CatalogPhraseSlotEmit("card.view", "dash.view.card", "view ", ["card", "view"]),
        ],
        new Dictionary<string, string> { ["card"] = "Card", ["view"] = "View" });

        var catalog = CommandCatalogIndex.FromDescriptors(
        [
            CommandDescriptors.Describe("dash.view.card")
                .Path("view heatmap_card heatmap")
                .Help("Heatmap view")
                .Group("View")
                .Build(),
            CommandDescriptors.Describe("dash.view.card")
                .Path("view heatmap_card line")
                .Help("Line view")
                .Group("View")
                .Build(),
        ]);

        var items = CatalogPathCompletion.GetSuggestions(catalog, "view ", phraseSlots);
        var cardItem = items.First(item => item.StepSegment == "heatmap_card");

        Assert.Equal("dash.view.card", cardItem.CommandId);
        Assert.Equal("card", cardItem.ActiveSlot);
        Assert.Equal("Card", cardItem.SlotLabel);
    }
}
