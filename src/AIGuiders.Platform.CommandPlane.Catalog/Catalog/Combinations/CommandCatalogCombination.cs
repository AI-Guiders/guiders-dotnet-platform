#nullable enable

using AIGuiders.Platform.Combinations;
using AIGuiders.Platform.Combinations.Overlay;
using AIGuiders.Platform.CommandPlane;

namespace AIGuiders.Platform.Combinations.Catalog;

public static class CommandCatalogOverlay
{
    public static OverlayPolicy<CommandCatalogIndex> ShipFirst { get; } =
        OverlayProfile.For<CommandCatalogIndex>("catalog.ship-first", CombinationSemantics.ShipFirst)
            .Rule(static (baseline, overlay) => baseline.Merge(overlay))
            .Build();
}

public static class CommandCatalogCombinators
{
    public static CombinationSemantics Semantics => CommandCatalogOverlay.ShipFirst.Semantics;

    public static Combinator<CommandCatalogIndex> ShipFirst { get; } =
        CommandCatalogOverlay.ShipFirst.Combinator;
}

public static class CommandCatalogCombination
{
    public static CommandCatalogIndex Compose(params ICommandSource[] sources) =>
        Compose((IEnumerable<ICommandSource>)sources);

    public static CommandCatalogIndex Compose(IEnumerable<ICommandSource> sources) =>
        OrderedCombination.FoldLayers(
            sources,
            CommandCatalogIndex.Empty,
            static source => CommandCatalogIndex.FromDescriptors(source.Load()),
            CommandCatalogCombinators.ShipFirst);
}
