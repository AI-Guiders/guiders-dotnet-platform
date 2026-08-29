#nullable enable
using System.Text.Json.Serialization;

namespace AIGuiders.Platform.CommandPlane.Conformance;

public sealed record SlashSpecDocument(
    [property: JsonPropertyName("version")] int Version,
    [property: JsonPropertyName("surface")] string Surface,
    [property: JsonPropertyName("tier")] string? Tier,
    [property: JsonPropertyName("source")] string? Source,
    [property: JsonPropertyName("catalogs")] IReadOnlyDictionary<string, IReadOnlyList<SlashSpecCatalogEntry>> Catalogs,
    [property: JsonPropertyName("pickerStubs")] IReadOnlyDictionary<string, SlashSpecPickerStub>? PickerStubs,
    [property: JsonPropertyName("vectors")] IReadOnlyList<SlashSpecVector> Vectors);

public sealed record SlashSpecCatalogEntry(
    [property: JsonPropertyName("domain")] string Domain,
    [property: JsonPropertyName("object")] string Object,
    [property: JsonPropertyName("intent")] string Intent,
    [property: JsonPropertyName("commandId")] string CommandId,
    [property: JsonPropertyName("path")] string Path,
    [property: JsonPropertyName("pathAliases")] IReadOnlyList<string>? PathAliases,
    [property: JsonPropertyName("help")] string? Help,
    [property: JsonPropertyName("group")] string? Group,
    [property: JsonPropertyName("argTail")] string? ArgTail,
    [property: JsonPropertyName("argHint")] string? ArgHint,
    [property: JsonPropertyName("argPickerChoices")] IReadOnlyList<SlashSpecPickerChoice>? ArgPickerChoices);

public sealed record SlashSpecPickerChoice(
    [property: JsonPropertyName("value")] string Value,
    [property: JsonPropertyName("label")] string? Label,
    [property: JsonPropertyName("hint")] string? Hint);

public sealed record SlashSpecPickerStub(
    [property: JsonPropertyName("choices")] IReadOnlyList<SlashSpecPickerChoice> Choices);

public sealed record SlashSpecVector(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("catalog")] string Catalog,
    [property: JsonPropertyName("body")] string Body,
    [property: JsonPropertyName("expect")] SlashSpecExpectation Expect);

public sealed record SlashSpecExpectation(
    [property: JsonPropertyName("suggestions")] SlashSpecSuggestionsExpectation? Suggestions,
    [property: JsonPropertyName("guidance")] SlashSpecGuidanceExpectation? Guidance);

public sealed record SlashSpecSuggestionsExpectation(
    [property: JsonPropertyName("items")] IReadOnlyList<SlashSpecCompletionItem>? Items,
    [property: JsonPropertyName("stepSegments")] IReadOnlyList<string>? StepSegments);

public sealed record SlashSpecCompletionItem(
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("insertText")] string InsertText,
    [property: JsonPropertyName("slashPath")] string SlashPath,
    [property: JsonPropertyName("help")] string Help,
    [property: JsonPropertyName("group")] string? Group,
    [property: JsonPropertyName("stepSegment")] string? StepSegment,
    [property: JsonPropertyName("pickValue")] string? PickValue);

public sealed record SlashSpecGuidanceExpectation(
    [property: JsonPropertyName("mode")] string Mode,
    [property: JsonPropertyName("breadcrumbContains")] string? BreadcrumbContains,
    [property: JsonPropertyName("placeholderContains")] string? PlaceholderContains,
    [property: JsonPropertyName("hint")] string? Hint);
