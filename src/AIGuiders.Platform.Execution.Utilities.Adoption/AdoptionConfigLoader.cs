using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIGuiders.Platform.Execution.Utilities.Adoption;

public static class AdoptionConfigLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public static AdoptionConfig Load(string planetsJsonPath, string hyperlaneMapJsonPath)
    {
        var planets = LoadPlanets(planetsJsonPath);
        var rules = LoadHyperlaneRules(hyperlaneMapJsonPath);
        return new AdoptionConfig(planets, rules);
    }

    public static IReadOnlyList<PlanetConfig> LoadPlanets(string path)
    {
        var json = File.ReadAllText(path);
        var doc = JsonSerializer.Deserialize<PlanetsDocument>(json, JsonOptions)
            ?? throw new InvalidOperationException($"Failed to parse planets config: {path}");
        return doc.Planets
            .Select(p => new PlanetConfig(p.Id, p.Name, p.Root, p.IssuesUrl))
            .ToList();
    }

    public static IReadOnlyList<HyperlaneRule> LoadHyperlaneRules(string path)
    {
        var json = File.ReadAllText(path);
        var doc = JsonSerializer.Deserialize<HyperlaneMapDocument>(json, JsonOptions)
            ?? throw new InvalidOperationException($"Failed to parse hyperlane map: {path}");
        return doc.Rules
            .OrderByDescending(r => r.Prefix.Length)
            .Select(r => new HyperlaneRule(r.Prefix, r.Hyperlane, r.PortHint))
            .ToList();
    }

    private sealed class PlanetsDocument
    {
        [JsonPropertyName("planets")]
        public List<PlanetEntry> Planets { get; init; } = [];
    }

    private sealed class PlanetEntry
    {
        [JsonPropertyName("id")]
        public string Id { get; init; } = "";

        [JsonPropertyName("name")]
        public string Name { get; init; } = "";

        [JsonPropertyName("root")]
        public string Root { get; init; } = "";

        [JsonPropertyName("issuesUrl")]
        public string? IssuesUrl { get; init; }
    }

    private sealed class HyperlaneMapDocument
    {
        [JsonPropertyName("rules")]
        public List<HyperlaneRuleEntry> Rules { get; init; } = [];
    }

    private sealed class HyperlaneRuleEntry
    {
        [JsonPropertyName("prefix")]
        public string Prefix { get; init; } = "";

        [JsonPropertyName("hyperlane")]
        public string Hyperlane { get; init; } = "";

        [JsonPropertyName("portHint")]
        public string PortHint { get; init; } = "dotnet-nuget";
    }
}
