#nullable enable

using AIGuiders.Platform.Combinations;
using AIGuiders.Platform.CommandPlane.Binding;

namespace AIGuiders.Platform.Combinations.Binding;

public static class BindingCombinators
{
    public static CombinationSemantics Semantics => CombinationSemantics.OverlayWins;

    /// <summary>Later binding layers override the same binding key (ADR-0017).</summary>
    public static Combinator<BindingCatalogIndex> OverlayWins { get; } = static (baseline, overlay) =>
        baseline.Merge(overlay);
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
