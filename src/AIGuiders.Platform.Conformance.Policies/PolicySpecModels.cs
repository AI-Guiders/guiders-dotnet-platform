using AIGuiders.Platform.IntermediateRepresentation.Binding;
#nullable enable
using System.Text.Json;
using System.Text.Json.Serialization;
using AIGuiders.Platform.Combinations;

namespace AIGuiders.Platform.Conformance.Policies;

public sealed record PolicySpecDocument(
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("policy")] string Policy,
    [property: JsonPropertyName("semantics")] CombinationSemantics Semantics,
    [property: JsonPropertyName("version")] int Version,
    [property: JsonPropertyName("source")] string? Source,
    [property: JsonPropertyName("vectors")] IReadOnlyList<PolicySpecVector> Vectors);

public sealed record PolicySpecVector(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("baseline")] JsonElement? Baseline,
    [property: JsonPropertyName("overlay")] JsonElement? Overlay,
    [property: JsonPropertyName("expect")] JsonElement? Expect);

public sealed record SlashPathWire(
    [property: JsonPropertyName("path")] string Path,
    [property: JsonPropertyName("commandId")] string CommandId);

public sealed record SlashLayerWire(
    [property: JsonPropertyName("paths")] IReadOnlyList<SlashPathWire>? Paths);

public sealed record SlashExpectWire(
    [property: JsonPropertyName("paths")] IReadOnlyDictionary<string, string>? Paths);

public sealed record BindingEntryWire(
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("gesture")] string Gesture);

public sealed record BindingLayerWire(
    [property: JsonPropertyName("bindings")] IReadOnlyList<BindingEntryWire>? Bindings);

public sealed record BindingExpectWire(
    [property: JsonPropertyName("bindings")] IReadOnlyDictionary<string, string>? Bindings);
