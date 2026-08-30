namespace AIGuiders.Platform.Utilities.Adoption;

public static class AdoptionAllianceBuilder
{
    public static AdoptionFactSet CollectFacts(
        PlanetConfig planet,
        string configBaseDirectory,
        IEnumerable<IAdoptionSource> sources)
    {
        var root = Path.GetFullPath(Path.Combine(configBaseDirectory, planet.Root));
        var facts = new AdoptionFactSet(planet.Id, planet.Name, planet.IssuesUrl);
        if (!Directory.Exists(root))
            return facts;

        var context = new AdoptionSourceContext(root);
        foreach (var source in sources)
            facts.Merge(source.Read(context));

        return facts;
    }

    public static IReadOnlyList<PlanetAdoptionRow> BuildPlanet(
        PlanetConfig planet,
        string configBaseDirectory,
        IReadOnlyList<HyperlaneRule> hyperlaneRules,
        IEnumerable<IAdoptionSource> sources)
    {
        var facts = CollectFacts(planet, configBaseDirectory, sources);
        return AdoptionHyperlaneProjector.Project(facts, hyperlaneRules);
    }

    public static IReadOnlyList<PlanetAdoptionRow> BuildAll(
        AdoptionConfig config,
        string configBaseDirectory,
        IEnumerable<IAdoptionSource> sources)
    {
        var rows = new List<PlanetAdoptionRow>();
        foreach (var planet in config.Planets)
            rows.AddRange(BuildPlanet(planet, configBaseDirectory, config.HyperlaneRules, sources));
        return rows;
    }
}
