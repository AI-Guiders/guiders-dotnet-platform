#nullable enable

using AIGuiders.Platform.Combinations;
using AIGuiders.Platform.CommandPlane;

namespace AIGuiders.Platform.Combinations.Slash;

public static class SlashCombinators
{
    public static CombinationSemantics Semantics => CombinationSemantics.ShipFirst;

    /// <summary>Baseline slash paths win; overlay adds routes only (ADR-0015 ship + extension).</summary>
    public static Combinator<SlashCatalogIndex> ShipFirst { get; } = static (baseline, overlay) =>
        baseline.Merge(overlay);
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
