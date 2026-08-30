#nullable enable

using AIGuiders.Platform.Combinations;
using AIGuiders.Platform.Combinations.Overlay;
using AIGuiders.Platform.CommandPlane;

namespace AIGuiders.Platform.Combinations.Slash;

public static class SlashOverlay
{
    public static OverlayPolicy<SlashCatalogIndex> ShipFirst { get; } =
        OverlayProfile.For<SlashCatalogIndex>("slash.ship-first", CombinationSemantics.ShipFirst)
            .Rule(static (baseline, overlay) => baseline.Merge(overlay))
            .Build();
}

public static class SlashCombinators
{
    public static CombinationSemantics Semantics => SlashOverlay.ShipFirst.Semantics;

    public static Combinator<SlashCatalogIndex> ShipFirst { get; } =
        SlashOverlay.ShipFirst.Combinator;
}

public static class SlashCatalogCombination
{
    public static SlashCatalogIndex Compose(params ICommandSource[] sources) =>
        Compose((IEnumerable<ICommandSource>)sources);

    public static SlashCatalogIndex Compose(IEnumerable<ICommandSource> sources) =>
        OrderedCombination.FoldLayers(
            sources,
            SlashCatalogIndex.Empty,
            static source => SlashCatalogIndex.FromDescriptors(source.Load()),
            SlashCombinators.ShipFirst);
}
