#nullable enable

using ModelingNavPolicy = AIGuiders.Platform.Modeling.Navigation.Policy;

namespace AIGuiders.Platform.Navigation.Policy;

/// <summary>
/// GUIDERS-FSHARP-ADR-0003 §4.8 cutover: kind filter SSOT via <see cref="ModelingNavPolicy.KindFilter"/> (Modeling.Navigation.Policy).
/// Non-empty <c>includeKinds</c> is a whitelist; <c>excludeKinds</c> subtracts. Unknown tokens in either list are ignored.
/// </summary>
public readonly struct NavigationKindFilter
{
    readonly ModelingNavPolicy.KindFilter.Filter _inner;

    NavigationKindFilter(ModelingNavPolicy.KindFilter.Filter inner) => _inner = inner;

    /// <summary><c>null</c> when no whitelist (all kinds except excluded).</summary>
    public IReadOnlyList<string>? EffectiveIncludeKinds
    {
        get
        {
            var include = ModelingNavPolicy.KindFilter.effectiveInclude(_inner);
            return FSharpInterop.OptReadOnlyStringList(include) is { Count: > 0 } list
                ? list.OrderBy(x => x, StringComparer.Ordinal).ToList()
                : null;
        }
    }

    public IReadOnlyList<string> EffectiveExcludeKinds
    {
        get
        {
            var exclude = FSharpInterop.ToReadOnlyList(ModelingNavPolicy.KindFilter.effectiveExclude(_inner));
            return exclude.Count == 0
                ? Array.Empty<string>()
                : exclude.OrderBy(x => x, StringComparer.Ordinal).ToList();
        }
    }

    public static NavigationKindFilter Create(IReadOnlyList<string>? includeKinds, IReadOnlyList<string>? excludeKinds) =>
        new(ModelingNavPolicy.KindFilter.create(
            FSharpInterop.OptFSharpStringList(includeKinds),
            FSharpInterop.OptFSharpStringList(excludeKinds)));

    public bool Allows(string kind) =>
        !string.IsNullOrEmpty(kind) && ModelingNavPolicy.KindFilter.allows(_inner, kind);
}
