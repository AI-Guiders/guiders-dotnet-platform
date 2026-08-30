using System.Text.Json;
using System.Xml.Linq;

namespace AIGuiders.Platform.Utilities.Adoption;

public static class PlanetAdoptionScanner
{
    private static readonly HashSet<string> SkipDirs = new(StringComparer.OrdinalIgnoreCase)
    {
        "bin", "obj", "node_modules", ".git", ".cdp",
    };

    public static IReadOnlyList<PlanetAdoptionRow> ScanPlanet(
        PlanetConfig planet,
        string configBaseDirectory,
        IReadOnlyList<HyperlaneRule> hyperlaneRules)
    {
        var root = Path.GetFullPath(Path.Combine(configBaseDirectory, planet.Root));
        if (!Directory.Exists(root))
            return [];

        var nugetPins = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var projectRefs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var csproj in EnumerateFiles(root, "*.csproj"))
            ScanCsproj(csproj, nugetPins, projectRefs);

        var specTags = CollectConformanceSpecs(root);
        var hasJsConformance = CollectJsConformance(root);

        var hyperlaneBuckets = new Dictionary<string, (HashSet<string> Packages, string Port)>(StringComparer.OrdinalIgnoreCase);

        foreach (var (packageId, version) in nugetPins)
        {
            var rule = MatchHyperlane(packageId, hyperlaneRules);
            if (rule is null)
                continue;

            var port = projectRefs.Contains(packageId) ? "dotnet-project-ref" : $"dotnet-nuget ({version})";
            if (!string.IsNullOrEmpty(rule.PortHint) && rule.PortHint != "dotnet-nuget")
                port = projectRefs.Contains(packageId) ? "dotnet-project-ref" : rule.PortHint;

            if (!hyperlaneBuckets.TryGetValue(rule.Hyperlane, out var bucket))
            {
                bucket = (new HashSet<string>(StringComparer.OrdinalIgnoreCase), port);
                hyperlaneBuckets[rule.Hyperlane] = bucket;
            }

            bucket.Packages.Add(packageId);
            if (projectRefs.Contains(packageId))
                hyperlaneBuckets[rule.Hyperlane] = (bucket.Packages, "dotnet-project-ref");
        }

        if (hasJsConformance && !hyperlaneBuckets.ContainsKey("Conformance"))
            hyperlaneBuckets["Conformance"] = (new HashSet<string> { "@aiguiders/conformance" }, "js-native");

        var rows = hyperlaneBuckets
            .OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
            .Select(kv => new PlanetAdoptionRow(
                planet.Id,
                planet.Name,
                kv.Key,
                kv.Value.Port,
                string.Join(", ", kv.Value.Packages.OrderBy(p => p, StringComparer.OrdinalIgnoreCase)),
                string.Join(", ", specTags.OrderBy(s => s, StringComparer.OrdinalIgnoreCase)),
                planet.IssuesUrl))
            .ToList();

        if (rows.Count == 0 && specTags.Count > 0)
        {
            rows.Add(new PlanetAdoptionRow(
                planet.Id,
                planet.Name,
                "(specs only)",
                hasJsConformance ? "js-native" : "vectors-embed",
                "—",
                string.Join(", ", specTags.OrderBy(s => s, StringComparer.OrdinalIgnoreCase)),
                planet.IssuesUrl));
        }

        return rows;
    }

    public static IReadOnlyList<PlanetAdoptionRow> ScanAll(AdoptionConfig config, string configBaseDirectory)
    {
        var rows = new List<PlanetAdoptionRow>();
        foreach (var planet in config.Planets)
            rows.AddRange(ScanPlanet(planet, configBaseDirectory, config.HyperlaneRules));
        return rows;
    }

    private static HyperlaneRule? MatchHyperlane(string packageId, IReadOnlyList<HyperlaneRule> rules)
    {
        foreach (var rule in rules)
        {
            if (packageId.StartsWith(rule.Prefix, StringComparison.OrdinalIgnoreCase))
                return rule;
        }

        return null;
    }

    private static void ScanCsproj(string path, Dictionary<string, string> nugetPins, HashSet<string> projectRefs)
    {
        XDocument doc;
        try
        {
            doc = XDocument.Load(path, LoadOptions.PreserveWhitespace);
        }
        catch
        {
            return;
        }

        foreach (var el in doc.Descendants())
        {
            if (!el.Name.LocalName.Equals("PackageReference", StringComparison.Ordinal))
                continue;

            var include = el.Attribute("Include")?.Value ?? el.Attribute("Update")?.Value;
            if (string.IsNullOrWhiteSpace(include) || !include.StartsWith("AIGuiders.Platform.", StringComparison.Ordinal))
                continue;

            var version = el.Attribute("Version")?.Value
                ?? el.Element(el.Name.Namespace + "Version")?.Value
                ?? "?";
            nugetPins[include] = version;
        }

        foreach (var el in doc.Descendants())
        {
            if (!el.Name.LocalName.Equals("ProjectReference", StringComparison.Ordinal))
                continue;

            var include = el.Attribute("Include")?.Value;
            if (string.IsNullOrWhiteSpace(include))
                continue;

            var fileName = Path.GetFileNameWithoutExtension(include);
            if (!fileName.StartsWith("AIGuiders.Platform.", StringComparison.Ordinal))
                continue;

            projectRefs.Add(fileName);
            nugetPins.TryAdd(fileName, "project");
        }
    }

    private static HashSet<string> CollectConformanceSpecs(string root)
    {
        var specs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in EnumerateFiles(root, "*.spec.json"))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            if (name.Contains("spec", StringComparison.OrdinalIgnoreCase))
                specs.Add(name.Replace(".spec", "", StringComparison.OrdinalIgnoreCase));
        }

        return specs;
    }

    private static bool CollectJsConformance(string root)
    {
        foreach (var pkg in EnumerateFiles(root, "package.json"))
        {
            try
            {
                using var stream = File.OpenRead(pkg);
                using var doc = JsonDocument.Parse(stream);
                if (doc.RootElement.TryGetProperty("dependencies", out var deps)
                    && deps.EnumerateObject().Any(p => p.Name.StartsWith("@aiguiders/", StringComparison.Ordinal)))
                    return true;
                if (doc.RootElement.TryGetProperty("devDependencies", out var devDeps)
                    && devDeps.EnumerateObject().Any(p => p.Name.StartsWith("@aiguiders/", StringComparison.Ordinal)))
                    return true;
            }
            catch
            {
                // skip invalid package.json
            }
        }

        return false;
    }

    private static IEnumerable<string> EnumerateFiles(string root, string pattern)
    {
        IEnumerable<string> Walk(string dir)
        {
            foreach (var sub in Directory.EnumerateDirectories(dir))
            {
                var name = Path.GetFileName(sub);
                if (SkipDirs.Contains(name))
                    continue;
                foreach (var f in Walk(sub))
                    yield return f;
            }

            foreach (var file in Directory.EnumerateFiles(dir, pattern))
                yield return file;
        }

        return Walk(root);
    }
}
