#nullable enable

using AIGuiders.Platform.Authoring.Command.Bundles;
using AIGuiders.Platform.Authoring.Command.Catalog;
using AIGuiders.Platform.Authoring.Core;
using AIGuiders.Platform.Execution.CommandPlane.Catalog.CodeGen;
using Xunit;

namespace AIGuiders.Platform.Authoring.Tests;

public sealed class CatalogPhraseSlotIndexTests
{
    [Fact]
    public void FromDocument_resolves_active_slot_for_view_phrase()
    {
        var doc = LoadDashCatalog();
        var index = CatalogPhraseSlotIndex.FromDocument(doc);

        Assert.Equal("card", index.ResolveActiveSlot("view ", commandKey: "card.view"));
        Assert.Equal("view", index.ResolveActiveSlot("view heatmap_card ", commandKey: "card.view"));
        Assert.Null(index.ResolveActiveSlot("view heatmap_card heatmap", commandKey: "card.view"));
    }

    [Fact]
    public void FromDocument_reads_bound_card_before_view()
    {
        var doc = LoadDashCatalog();
        var index = CatalogPhraseSlotIndex.FromDocument(doc);

        Assert.Equal(
            "heatmap_card",
            index.ReadBoundSlotValue("view heatmap_card ", commandKey: "card.view", "card"));
    }

    [Fact]
    public void FromEmitted_matches_FromDocument()
    {
        var doc = LoadDashCatalog();
        var runtime = CatalogPhraseSlotIndex.FromDocument(doc);
        var code = CatalogCatalogEmitter.EmitCSharp(doc, "DashSpec.Generated", "DashCatalog");

        Assert.Contains("PhraseSlotCommands", code, StringComparison.Ordinal);
        Assert.Contains("PhraseSlotLabels", code, StringComparison.Ordinal);
        Assert.Contains("PhraseSlots =>", code, StringComparison.Ordinal);
    }

    static CatalogDocument LoadDashCatalog()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "Authoring", "dash.catalog.gdl");
        var result = CatalogParser.ParseFile(path, CatalogBundleLibrary.Federation);
        Assert.NotNull(result.Document);
        return result.Document;
    }
}
