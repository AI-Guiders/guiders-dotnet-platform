#nullable enable
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.FSharp.Collections;

namespace AIGuiders.Platform.Conformance.Policies;

public static class PolicySpecLoader
{
    public static PolicySpecDocument LoadJson(string json) =>
        ToDocument(JsonSerializer.Deserialize<PolicySpecWireDocument>(json, JsonOptions)
            ?? throw new InvalidOperationException("Policy spec JSON deserialized to null."));

    public static PolicySpecDocument LoadToml(string toml) =>
        LoadJson(PolicySpecFormats.TomlToPolicyJson(toml));

    public static PolicySpecDocument LoadFile(string path)
    {
        var text = File.ReadAllText(path);
        return Path.GetExtension(path).Equals(".toml", StringComparison.OrdinalIgnoreCase)
            ? LoadToml(text)
            : LoadJson(text);
    }

    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower,
        Converters = { new JsonStringEnumConverter() },
    };

    static PolicySpecDocument ToDocument(PolicySpecWireDocument wire) => new()
    {
        Kind = wire.Kind,
        Policy = wire.Policy,
        Semantics = wire.Semantics,
        Version = wire.Version,
        Source = wire.Source ?? "",
        Vectors = ListModule.OfSeq(wire.Vectors.Select(ToVector)),
    };

    static PolicySpecVector ToVector(PolicySpecWireVector wire) => new()
    {
        Id = wire.Id,
        BaselineJson = wire.Baseline?.GetRawText() ?? "null",
        OverlayJson = wire.Overlay?.GetRawText() ?? "null",
        ExpectJson = wire.Expect?.GetRawText() ?? "null",
    };

    sealed record PolicySpecWireDocument(
        [property: JsonPropertyName("kind")] string Kind,
        [property: JsonPropertyName("policy")] string Policy,
        [property: JsonPropertyName("semantics")] string Semantics,
        [property: JsonPropertyName("version")] int Version,
        [property: JsonPropertyName("source")] string? Source,
        [property: JsonPropertyName("vectors")] IReadOnlyList<PolicySpecWireVector> Vectors);

    sealed record PolicySpecWireVector(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("baseline")] JsonElement? Baseline,
        [property: JsonPropertyName("overlay")] JsonElement? Overlay,
        [property: JsonPropertyName("expect")] JsonElement? Expect);
}
