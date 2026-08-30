#nullable enable

using AIGuiders.Platform.Combinations;
using AIGuiders.Platform.Combinations.Overlay;
using AIGuiders.Platform.CommandPlane.Binding;

namespace AIGuiders.Platform.Combinations.Binding;

public static class BindingOverlay
{
    public static OverlayPolicy<BindingCatalogIndex> OverlayWins { get; } =
        OverlayProfile.For<BindingCatalogIndex>("binding.overlay-wins", CombinationSemantics.OverlayWins)
            .Rule(static (baseline, overlay) => baseline.Merge(overlay))
            .Build();
}

public static class BindingCombinators
{
    public static CombinationSemantics Semantics => BindingOverlay.OverlayWins.Semantics;

    public static Combinator<BindingCatalogIndex> OverlayWins { get; } =
        BindingOverlay.OverlayWins.Combinator;
}

public static class BindingCatalogCombination
{
    public static BindingCatalogIndex Compose(params IBindingSource[] sources) =>
        Compose((IEnumerable<IBindingSource>)sources);

    public static BindingCatalogIndex Compose(IEnumerable<IBindingSource> sources) =>
        OrderedCombination.FoldLayers(
            sources,
            BindingCatalogIndex.Empty,
            static source => BindingCatalogIndex.FromDescriptors(source.Load()),
            BindingCombinators.OverlayWins);
}
