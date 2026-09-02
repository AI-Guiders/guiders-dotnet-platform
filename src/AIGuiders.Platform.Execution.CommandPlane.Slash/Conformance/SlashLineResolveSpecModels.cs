#nullable enable
using System.Text.Json.Serialization;

namespace AIGuiders.Platform.Execution.CommandPlane.Conformance;

public sealed record SlashLineResolveSpecDocument(
    [property: JsonPropertyName("version")] int Version,
    [property: JsonPropertyName("surface")] string Surface,
    [property: JsonPropertyName("tier")] string? Tier,
    [property: JsonPropertyName("source")] string? Source,
    [property: JsonPropertyName("catalogs")] IReadOnlyDictionary<string, IReadOnlyList<SlashSpecCatalogEntry>> Catalogs,
    [property: JsonPropertyName("vectors")] IReadOnlyList<SlashLineResolveVector> Vectors);

public sealed record SlashLineResolveVector(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("catalog")] string Catalog,
    [property: JsonPropertyName("slashLine")] string SlashLine,
    [property: JsonPropertyName("expect")] SlashLineResolveExpectation Expect);

public sealed record SlashLineResolveExpectation(
    [property: JsonPropertyName("canonicalPath")] string CanonicalPath,
    [property: JsonPropertyName("argTail")] string? ArgTail,
    [property: JsonPropertyName("isRunnable")] bool IsRunnable,
    [property: JsonPropertyName("isCatalogMatch")] bool IsCatalogMatch,
    [property: JsonPropertyName("argTailKind")] string? ArgTailKind);
