#nullable enable

using AIGuiders.Platform.Navigation;
using ModelingNavPolicy = AIGuiders.Platform.Modeling.Navigation.Policy;

namespace AIGuiders.Platform.Navigation.Policy;

/// <summary>GUIDERS-FSHARP-ADR-0003 §4.8 cutover: explore profile SSOT via <see cref="ToModel"/> (Modeling.Navigation.Policy).</summary>
public sealed record NavigationProfile
{
    public string? Preset { get; init; }
    public int MaxRelated { get; init; } = 24;
    public int MaxNodes { get; init; } = 12;
    public int MaxEdges { get; init; } = 24;
    public bool WithUsages { get; init; }
    /// <summary>Effective include kinds after preset merge (optional).</summary>
    public IReadOnlyList<string>? IncludeKinds { get; init; }
    /// <summary>Effective exclude kinds after preset merge (optional).</summary>
    public IReadOnlyList<string>? ExcludeKinds { get; init; }

    public static NavigationProfile ExploreDefault { get; } =
        FromModel(ModelingNavPolicy.Profile.exploreDefault);

    public static NavigationProfile PeersOnly { get; } =
        FromModel(ModelingNavPolicy.Profile.peersOnly);

    /// <summary>Build profile from MCP/CSX explore args (preset + request overrides).</summary>
    public static NavigationProfile FromExplore(
        string? preset,
        int? maxRelated,
        IReadOnlyList<string>? requestInclude,
        IReadOnlyList<string>? requestExclude) =>
        FromModel(ModelingNavPolicy.Profile.fromExplore(
            FSharpInterop.OptString(preset),
            FSharpInterop.OptInt(maxRelated),
            FSharpInterop.OptFSharpStringList(requestInclude),
            FSharpInterop.OptFSharpStringList(requestExclude)));

    public NavigationSceneCaps ToCaps() =>
        NavigationSceneCaps.FromModel(ModelingNavPolicy.Profile.toCaps(ToModel()));

    public ModelingNavPolicy.NavigationProfile ToModel() => new()
    {
        Preset = FSharpInterop.OptString(Preset),
        MaxRelated = MaxRelated,
        MaxNodes = MaxNodes,
        MaxEdges = MaxEdges,
        WithUsages = WithUsages,
        IncludeKinds = FSharpInterop.OptFSharpStringList(IncludeKinds),
        ExcludeKinds = FSharpInterop.OptFSharpStringList(ExcludeKinds),
    };

    public static NavigationProfile FromModel(ModelingNavPolicy.NavigationProfile model) => new()
    {
        Preset = FSharpInterop.OptString(model.Preset),
        MaxRelated = model.MaxRelated,
        MaxNodes = model.MaxNodes,
        MaxEdges = model.MaxEdges,
        WithUsages = model.WithUsages,
        IncludeKinds = FSharpInterop.OptReadOnlyStringList(model.IncludeKinds),
        ExcludeKinds = FSharpInterop.OptReadOnlyStringList(model.ExcludeKinds),
    };
}
