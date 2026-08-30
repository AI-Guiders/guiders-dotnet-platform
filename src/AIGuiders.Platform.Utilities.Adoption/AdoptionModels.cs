namespace AIGuiders.Platform.Utilities.Adoption;

public sealed record PlanetConfig(
    string Id,
    string Name,
    string Root,
    string? IssuesUrl);

public sealed record HyperlaneRule(
    string Prefix,
    string Hyperlane,
    string PortHint);

public sealed record AdoptionConfig(
    IReadOnlyList<PlanetConfig> Planets,
    IReadOnlyList<HyperlaneRule> HyperlaneRules);

public sealed record PlanetAdoptionRow(
    string PlanetId,
    string PlanetName,
    string Hyperlane,
    string Port,
    string Packages,
    string SpecTags,
    string? IssuesUrl);
