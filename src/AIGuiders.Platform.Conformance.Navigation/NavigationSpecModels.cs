#nullable enable
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIGuiders.Platform.Conformance.Navigation;

public sealed record NavigationSpecDocument(
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("surface")] string Surface,
    [property: JsonPropertyName("version")] int Version,
    [property: JsonPropertyName("source")] string? Source,
    [property: JsonPropertyName("vectors")] IReadOnlyList<NavigationSpecVector> Vectors);

public sealed record NavigationSpecVector(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("wire")] JsonElement Wire,
    [property: JsonPropertyName("profile")] JsonElement? Profile,
    [property: JsonPropertyName("expect")] JsonElement Expect);

public sealed record NavigationExpectWire(
    [property: JsonPropertyName("node_count")] int? NodeCount,
    [property: JsonPropertyName("kinds")] IReadOnlyList<string>? Kinds,
    [property: JsonPropertyName("excluded_kinds")] IReadOnlyList<string>? ExcludedKinds,
    [property: JsonPropertyName("max_kind_count")] IReadOnlyDictionary<string, int>? MaxKindCount);

public sealed record NavigationProfileWire(
    [property: JsonPropertyName("preset")] string? Preset,
    [property: JsonPropertyName("max_related")] int? MaxRelated,
    [property: JsonPropertyName("max_nodes")] int? MaxNodes,
    [property: JsonPropertyName("max_edges")] int? MaxEdges,
    [property: JsonPropertyName("with_usages")] bool? WithUsages);
