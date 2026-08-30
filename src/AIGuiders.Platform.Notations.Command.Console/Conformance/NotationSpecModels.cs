#nullable enable
using System.Text.Json.Serialization;

namespace AIGuiders.Platform.Notations.Conformance;

public sealed record NotationSpecDocument(
    [property: JsonPropertyName("version")] int Version,
    [property: JsonPropertyName("surface")] string Surface,
    [property: JsonPropertyName("tier")] string? Tier,
    [property: JsonPropertyName("source")] string? Source,
    [property: JsonPropertyName("vectors")] IReadOnlyList<NotationSpecVector> Vectors);

public sealed record NotationSpecVector(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("line")] string? Line,
    [property: JsonPropertyName("body")] string? Body,
    [property: JsonPropertyName("tail")] string? Tail,
    [property: JsonPropertyName("slashLine")] string? SlashLine,
    [property: JsonPropertyName("consoleLine")] string? ConsoleLine,
    [property: JsonPropertyName("expect")] NotationSpecExpectation Expect);

public sealed record NotationSpecExpectation(
    [property: JsonPropertyName("tokens")] IReadOnlyList<string>? Tokens,
    [property: JsonPropertyName("endsWithSpace")] bool? EndsWithSpace,
    [property: JsonPropertyName("slots")] IReadOnlyDictionary<string, string>? Slots,
    [property: JsonPropertyName("readerId")] string? ReaderId,
    [property: JsonPropertyName("canonicalPath")] string? CanonicalPath);
