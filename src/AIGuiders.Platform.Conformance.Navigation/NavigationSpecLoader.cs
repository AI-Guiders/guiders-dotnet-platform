#nullable enable
using System.Text.Json;
using System.Text.Json.Serialization;
using AIGuiders.Platform.Navigation.Policy;

namespace AIGuiders.Platform.Conformance.Navigation;

public static class NavigationSpecLoader
{
    public static NavigationSpecDocument LoadJson(string json) =>
        JsonSerializer.Deserialize<NavigationSpecDocument>(json, JsonOptions)
        ?? throw new InvalidOperationException("Navigation spec JSON deserialized to null.");

    public static NavigationSpecDocument LoadFile(string path) =>
        LoadJson(File.ReadAllText(path));

    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    public static NavigationProfile LoadProfile(JsonElement? profileNode)
    {
        if (profileNode is null or { ValueKind: JsonValueKind.Null or JsonValueKind.Undefined })
            return NavigationProfile.ExploreDefault;

        var wire = profileNode.Value.Deserialize<NavigationProfileWire>(JsonOptions);
        if (wire is null)
            return NavigationProfile.ExploreDefault;

        return new NavigationProfile
        {
            Preset = wire.Preset,
            MaxRelated = wire.MaxRelated ?? NavigationProfile.ExploreDefault.MaxRelated,
            MaxNodes = wire.MaxNodes ?? NavigationProfile.ExploreDefault.MaxNodes,
            MaxEdges = wire.MaxEdges ?? NavigationProfile.ExploreDefault.MaxEdges,
            WithUsages = wire.WithUsages ?? false,
        };
    }
}
