namespace AIGuiders.Platform.Utilities.Adoption;

/// <summary>IR → alliance rows (hyperlane grouping).</summary>
public static class AdoptionHyperlaneProjector
{
    public static IReadOnlyList<PlanetAdoptionRow> Project(
        AdoptionFactSet facts,
        IReadOnlyList<HyperlaneRule> hyperlaneRules)
    {
        var hyperlaneBuckets = new Dictionary<string, (HashSet<string> Packages, string Port)>(StringComparer.OrdinalIgnoreCase);
        var specTags = facts.Specs
            .Select(s => s.Tag)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var pin in facts.Pins.DistinctBy(p => p.PackageId, StringComparer.OrdinalIgnoreCase))
        {
            var rule = MatchHyperlane(pin.PackageId, hyperlaneRules);
            if (rule is null)
                continue;

            var port = FormatPort(pin, rule);
            if (!hyperlaneBuckets.TryGetValue(rule.Hyperlane, out var bucket))
            {
                bucket = (new HashSet<string>(StringComparer.OrdinalIgnoreCase), port);
                hyperlaneBuckets[rule.Hyperlane] = bucket;
            }

            bucket.Packages.Add(pin.PackageId);
            if (pin.PortKind is AdoptionPortKind.ProjectRef)
                hyperlaneBuckets[rule.Hyperlane] = (bucket.Packages, "dotnet-project-ref");
        }

        var specLine = specTags.Count == 0 ? "—" : string.Join(", ", specTags);
        var rows = hyperlaneBuckets
            .OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
            .Select(kv => new PlanetAdoptionRow(
                facts.PlanetId,
                facts.PlanetName,
                kv.Key,
                kv.Value.Port,
                string.Join(", ", kv.Value.Packages.OrderBy(p => p, StringComparer.OrdinalIgnoreCase)),
                specLine,
                facts.IssuesUrl))
            .ToList();

        if (rows.Count == 0 && specTags.Count > 0)
        {
            var hasJs = facts.Pins.Any(p => p.PortKind == AdoptionPortKind.NpmPackage);
            rows.Add(new PlanetAdoptionRow(
                facts.PlanetId,
                facts.PlanetName,
                "(specs only)",
                hasJs ? "js-native" : "vectors-embed",
                "—",
                specLine,
                facts.IssuesUrl));
        }

        return rows;
    }

    private static string FormatPort(AdoptionPin pin, HyperlaneRule rule) => pin.PortKind switch
    {
        AdoptionPortKind.ProjectRef => "dotnet-project-ref",
        AdoptionPortKind.NpmPackage => "js-native",
        AdoptionPortKind.NuGetPin => $"dotnet-nuget ({pin.Version ?? "?"})",
        _ => rule.PortHint,
    };

    private static HyperlaneRule? MatchHyperlane(string packageId, IReadOnlyList<HyperlaneRule> rules)
    {
        foreach (var rule in rules)
        {
            if (packageId.StartsWith(rule.Prefix, StringComparison.OrdinalIgnoreCase))
                return rule;
        }

        return null;
    }
}
