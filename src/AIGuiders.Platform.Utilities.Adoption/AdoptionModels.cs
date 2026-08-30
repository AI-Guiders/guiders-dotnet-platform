namespace AIGuiders.Platform.Utilities.Adoption;

public enum AdoptionPortKind
{
    NuGetPin,
    ProjectRef,
    NpmPackage,
    VectorsEmbed,
}

public sealed record AdoptionPin(
    string PackageId,
    string? Version,
    AdoptionPortKind PortKind);

public sealed record AdoptionSpecTag(string Tag);

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

/// <summary>IR: raw facts from Sources before hyperlane projection.</summary>
public sealed class AdoptionFactSet
{
    public AdoptionFactSet(string planetId, string planetName, string? issuesUrl)
    {
        PlanetId = planetId;
        PlanetName = planetName;
        IssuesUrl = issuesUrl;
    }

    public string PlanetId { get; }
    public string PlanetName { get; }
    public string? IssuesUrl { get; }
    public List<AdoptionPin> Pins { get; } = [];
    public List<AdoptionSpecTag> Specs { get; } = [];

    public void Merge(AdoptionPartialFacts partial)
    {
        Pins.AddRange(partial.Pins);
        Specs.AddRange(partial.Specs);
    }
}

public sealed record AdoptionPartialFacts(
    IReadOnlyList<AdoptionPin> Pins,
    IReadOnlyList<AdoptionSpecTag> Specs)
{
    public static AdoptionPartialFacts Empty { get; } = new([], []);
}

/// <summary>IR: projected alliance rows for Reports.</summary>
public sealed record PlanetAdoptionRow(
    string PlanetId,
    string PlanetName,
    string Hyperlane,
    string Port,
    string Packages,
    string SpecTags,
    string? IssuesUrl);
