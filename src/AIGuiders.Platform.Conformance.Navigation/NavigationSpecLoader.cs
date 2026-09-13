#nullable enable
using System.Text.Json;
using System.Text.Json.Serialization;
using AIGuiders.Platform.Navigation.Policy;
using Microsoft.FSharp.Core;

namespace AIGuiders.Platform.Conformance.Navigation;

public static class NavigationSpecLoader
{
    public static NavigationSpecDocument LoadJson(string json)
    {
        var wire = JsonSerializer.Deserialize<NavigationSpecJsonDocument>(json, JsonOptions)
            ?? throw new InvalidOperationException("Navigation spec JSON deserialized to null.");

        return new NavigationSpecDocument
        {
            Kind = wire.Kind,
            Surface = wire.Surface,
            Version = wire.Version,
            Source = wire.Source ?? "",
            Vectors = FSharpInterop.ToFSharpList(wire.Vectors.Select(ToModelVector).ToList()),
        };
    }

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

    public static NavigationProfile LoadProfileJson(string? profileJson)
    {
        if (string.IsNullOrWhiteSpace(profileJson))
            return NavigationProfile.ExploreDefault;

        var wire = JsonSerializer.Deserialize<NavigationProfileWire>(profileJson, JsonOptions);
        if (wire is null)
            return NavigationProfile.ExploreDefault;

        return new NavigationProfile
        {
            Preset = FSharpInterop.OptString(wire.Preset),
            MaxRelated = FSharpInterop.OptInt(wire.MaxRelated) ?? NavigationProfile.ExploreDefault.MaxRelated,
            MaxNodes = FSharpInterop.OptInt(wire.MaxNodes) ?? NavigationProfile.ExploreDefault.MaxNodes,
            MaxEdges = FSharpInterop.OptInt(wire.MaxEdges) ?? NavigationProfile.ExploreDefault.MaxEdges,
            WithUsages = wire.WithUsages is FSharpOption<bool> withUsagesOpt && FSharpOption<bool>.get_IsSome(withUsagesOpt) && withUsagesOpt.Value,
        };
    }

    public static NavigationExpectModel LoadExpectation(string expectJson)
    {
        var wire = JsonSerializer.Deserialize<NavigationExpectJsonWire>(expectJson, JsonOptions)
            ?? throw new InvalidOperationException("Navigation expect JSON deserialized to null.");
        return wire.ToModel();
    }

    static NavigationSpecVector ToModelVector(NavigationSpecJsonVector vector) => new()
    {
        Id = vector.Id,
        WireJson = vector.Wire.GetRawText(),
        ProfileJson = vector.Profile is { ValueKind: not JsonValueKind.Null and not JsonValueKind.Undefined } profile
            ? profile.GetRawText()
            : "",
        ExpectJson = vector.Expect.GetRawText(),
    };
}
