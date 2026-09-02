#nullable enable

using AIGuiders.Platform.Modeling.Catalog;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class CatalogKernelTests
{
    sealed record Row(string Id, string Label);

    sealed class RowProfile(CatalogIndexCollisionPolicy layer, CatalogIndexCollisionPolicy merge)
        : ICatalogProfile<Row, string, string>
    {
        public IEqualityComparer<string> KeyComparer => StringComparer.OrdinalIgnoreCase;

        public CatalogIndexCollisionPolicy LayerCollisionPolicy { get; } = layer;

        public CatalogIndexCollisionPolicy MergeCollisionPolicy { get; } = merge;

        public IEnumerable<(string, string)> Project(Row descriptor) =>
            [(descriptor.Id, descriptor.Label)];

        public string NormalizeKey(string key) => key.Trim();
    }

    [Fact]
    public void CatalogIndex_ship_first_skips_duplicate_keys_in_layer_and_merge()
    {
        var profile = new RowProfile(
            CatalogIndexCollisionPolicy.ShipFirst,
            CatalogIndexCollisionPolicy.ShipFirst);
        var baseline = CatalogIndex<string, string>.FromDescriptors(
            [new Row("a", "one"), new Row("a", "two")],
            profile);
        var overlay = CatalogIndex<string, string>.FromDescriptors([new Row("a", "three")], profile);

        var merged = baseline.MergeShipFirst(overlay);

        string value = default!;
        Assert.True(merged.TryGet("a", ref value));
        Assert.Equal("one", value);
    }

    [Fact]
    public void CatalogIndex_overlay_wins_overwrites_on_merge()
    {
        var profile = new RowProfile(
            CatalogIndexCollisionPolicy.OverlayWins,
            CatalogIndexCollisionPolicy.OverlayWins);
        var baseline = CatalogIndex<string, string>.FromDescriptors([new Row("a", "one")], profile);
        var overlay = CatalogIndex<string, string>.FromDescriptors([new Row("a", "two")], profile);

        var merged = baseline.MergeOverlayWins(overlay);

        string value = default!;
        Assert.True(merged.TryGet("a", ref value));
        Assert.Equal("two", value);
    }
}
