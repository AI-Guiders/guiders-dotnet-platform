#nullable enable

namespace AIGuiders.Platform.Navigation.Policy;

public sealed record NavigationProfile
{
    public string? Preset { get; init; }
    public int MaxRelated { get; init; } = 24;
    public int MaxNodes { get; init; } = 12;
    public int MaxEdges { get; init; } = 24;
    public bool WithUsages { get; init; }

    public static NavigationProfile ExploreDefault { get; } = new() { Preset = "explore_default", MaxRelated = 24 };

    public static NavigationProfile PeersOnly { get; } = new() { Preset = "peers_only", MaxRelated = 16 };

    public NavigationSceneCaps ToCaps() => new(
        MaxRelated,
        MaxNodes,
        MaxEdges,
        Preset,
        NavigationKindCaps.DefaultRelated);
}
