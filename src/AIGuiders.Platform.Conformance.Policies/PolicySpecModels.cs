#nullable enable
using System.Text.Json.Serialization;

namespace AIGuiders.Platform.Conformance.Policies;

public sealed record SlashLayerWire(
    [property: JsonPropertyName("paths")] IReadOnlyList<SlashPathWire>? Paths);

public sealed record SlashExpectWire(
    [property: JsonPropertyName("paths")] IReadOnlyDictionary<string, string>? Paths);

public sealed record BindingLayerWire(
    [property: JsonPropertyName("bindings")] IReadOnlyList<BindingEntryWire>? Bindings);

public sealed record BindingExpectWire(
    [property: JsonPropertyName("bindings")] IReadOnlyDictionary<string, string>? Bindings);
