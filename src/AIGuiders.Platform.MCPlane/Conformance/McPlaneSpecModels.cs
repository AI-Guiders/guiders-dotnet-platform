#nullable enable
using System.Text.Json.Serialization;

namespace AIGuiders.Platform.MCPlane.Conformance;

public sealed record McPlaneSpecDocument(
    [property: JsonPropertyName("version")] int Version,
    [property: JsonPropertyName("surface")] string Surface,
    [property: JsonPropertyName("tier")] string? Tier,
    [property: JsonPropertyName("source")] string? Source,
    [property: JsonPropertyName("vectors")] IReadOnlyList<McPlaneSpecVector> Vectors);

public sealed record McPlaneSpecVector(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("outcome")] McPlaneSpecOutcome? Outcome,
    [property: JsonPropertyName("next")] IReadOnlyList<McPlaneSpecNextHint>? Next,
    [property: JsonPropertyName("expect")] McPlaneSpecExpectation Expect);

public sealed record McPlaneSpecOutcome(
    [property: JsonPropertyName("raw")] string Raw,
    [property: JsonPropertyName("verb")] string Verb,
    [property: JsonPropertyName("ok")] bool Ok,
    [property: JsonPropertyName("pulse")] string? Pulse,
    [property: JsonPropertyName("reason")] string? Reason);

public sealed record McPlaneSpecNextHint(
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("commandId")] string? CommandId,
    [property: JsonPropertyName("toolName")] string? ToolName,
    [property: JsonPropertyName("label")] string? Label);

public sealed record McPlaneSpecExpectation(
    [property: JsonPropertyName("tier")] string? Tier,
    [property: JsonPropertyName("pulseMaxChars")] int? PulseMaxChars,
    [property: JsonPropertyName("pulseEndsWithEllipsis")] bool? PulseEndsWithEllipsis,
    [property: JsonPropertyName("outcomeIncluded")] bool? OutcomeIncluded,
    [property: JsonPropertyName("nextCount")] int? NextCount,
    [property: JsonPropertyName("nextKinds")] IReadOnlyList<string>? NextKinds);
