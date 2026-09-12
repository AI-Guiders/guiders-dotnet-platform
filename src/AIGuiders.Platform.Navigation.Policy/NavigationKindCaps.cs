#nullable enable

using ModelingNavPolicy = AIGuiders.Platform.Modeling.Navigation.Policy;

namespace AIGuiders.Platform.Navigation.Policy;

/// <summary>GUIDERS-FSHARP-ADR-0003 §4.8 cutover: default per-kind caps SSOT in Modeling.Navigation.Policy.</summary>
public static class NavigationKindCaps
{
    public static IReadOnlyDictionary<string, int> DefaultRelated { get; } =
        ModelingNavPolicy.KindCaps.defaultRelated.ToDictionary(
            pair => pair.Item1,
            pair => pair.Item2,
            StringComparer.Ordinal);
}
