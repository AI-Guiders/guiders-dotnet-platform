#nullable enable

namespace AIGuiders.Platform.Navigation.Policy;

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

    public static NavigationProfile ExploreDefault { get; } = new() { Preset = "explore_default", MaxRelated = 24 };

    public static NavigationProfile PeersOnly { get; } = new() { Preset = "peers_only", MaxRelated = 16 };

    /// <summary>Build profile from MCP/CSX explore args (preset + request overrides).</summary>
    public static NavigationProfile FromExplore(
        string? preset,
        int? maxRelated,
        IReadOnlyList<string>? requestInclude,
        IReadOnlyList<string>? requestExclude)
    {
        var (include, exclude, _) = NavigationPresetMerge.Merge(preset, requestInclude, requestExclude);
        return new NavigationProfile
        {
            Preset = preset,
            MaxRelated = maxRelated is > 0 ? maxRelated.Value : 24,
            IncludeKinds = include,
            ExcludeKinds = exclude is { Count: > 0 } ? exclude : null,
        };
    }

    public NavigationSceneCaps ToCaps() => new(
        MaxRelated,
        MaxNodes,
        MaxEdges,
        Preset,
        NavigationKindCaps.DefaultRelated);
}
