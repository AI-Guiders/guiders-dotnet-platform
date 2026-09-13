#nullable enable
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIGuiders.Platform.Conformance.Navigation;

/// <summary>JSON file wire shapes for spec loading only — SSOT models live in Modeling.Conformance.</summary>
internal sealed record NavigationSpecJsonDocument(
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("surface")] string Surface,
    [property: JsonPropertyName("version")] int Version,
    [property: JsonPropertyName("source")] string? Source,
    [property: JsonPropertyName("vectors")] IReadOnlyList<NavigationSpecJsonVector> Vectors);

internal sealed record NavigationSpecJsonVector(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("wire")] JsonElement Wire,
    [property: JsonPropertyName("profile")] JsonElement? Profile,
    [property: JsonPropertyName("expect")] JsonElement Expect);

internal sealed record NavigationExpectJsonWire(
    [property: JsonPropertyName("node_count")] int? NodeCount,
    [property: JsonPropertyName("kinds")] IReadOnlyList<string>? Kinds,
    [property: JsonPropertyName("excluded_kinds")] IReadOnlyList<string>? ExcludedKinds,
    [property: JsonPropertyName("max_kind_count")] IReadOnlyDictionary<string, int>? MaxKindCount)
{
    internal NavigationExpectModel ToModel() => new()
    {
        NodeCount = NodeCount ?? 0,
        Kinds = FSharpInterop.ToFSharpList(Kinds ?? Array.Empty<string>()),
        ExcludedKinds = FSharpInterop.ToFSharpList(ExcludedKinds ?? Array.Empty<string>()),
        MaxKindCounts = FSharpInterop.ToFSharpTupleList(MaxKindCount),
    };
}
