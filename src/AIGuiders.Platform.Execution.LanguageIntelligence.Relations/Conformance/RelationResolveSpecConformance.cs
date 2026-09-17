#nullable enable
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIGuiders.Platform.Execution.LanguageIntelligence.Relations.Conformance;

public static class RelationResolveSpecConformance
{
    public static RelationResolveSpecDocument Load(string json) =>
        JsonSerializer.Deserialize<RelationResolveSpecDocument>(json, JsonOptions)
        ?? throw new InvalidOperationException("Relation resolve spec JSON deserialized to null.");

    public static IReadOnlyList<string> ValidateDocument(RelationResolveSpecDocument spec)
    {
        var errors = new List<string>();
        foreach (var vector in spec.Vectors)
        {
            if (!TryValidateVector(vector, out var error))
                errors.Add($"[{vector.Id}] {error}");
        }

        return errors;
    }

    public static bool TryValidateVector(RelationResolveSpecVector vector, out string error)
    {
        error = "";
        if (vector.Wire is null)
            return Fail("wire is required.", out error);

        if (vector.Expect is null)
            return Fail("expect is required.", out error);

        BracketAnchorSpan span;
        try
        {
            span = LegacyBracketRelationWire.Parse(vector.Wire);
        }
        catch (ArgumentException ex)
        {
            error = ex.Message;
            return false;
        }

        var family = LegacyBracketRelationWire.ClassifyFamily(span, out var familyError);
        if (vector.Expect.Family is not null)
        {
            if (!Enum.TryParse<BracketAxisFamily>(vector.Expect.Family, ignoreCase: true, out var expectedFamily))
                return Fail($"unknown expect.family \"{vector.Expect.Family}\".", out error);

            if (family != expectedFamily)
            {
                error = $"family expected {expectedFamily}, got {family}.";
                return false;
            }
        }

        if (vector.Expect.FamilyError is not null && familyError != vector.Expect.FamilyError)
        {
            error = $"familyError expected \"{vector.Expect.FamilyError}\", got \"{familyError}\".";
            return false;
        }

        if (!SpanMatches(vector.Expect, span, out error))
            return false;

        return true;
    }

    static bool SpanMatches(RelationResolveSpecExpectation expect, BracketAnchorSpan actual, out string error)
    {
        error = "";
        if (expect.File is not null && expect.File != actual.File)
        {
            error = $"file expected \"{expect.File}\", got \"{actual.File}\".";
            return false;
        }

        if (expect.MemberKey is not null && expect.MemberKey != actual.MemberKey)
        {
            error = $"memberKey expected \"{expect.MemberKey}\", got \"{actual.MemberKey}\".";
            return false;
        }

        if (expect.LineStart is not null && expect.LineStart != actual.LineStart)
        {
            error = $"lineStart expected {expect.LineStart}, got {actual.LineStart}.";
            return false;
        }

        if (expect.LineEnd is not null && expect.LineEnd != actual.LineEnd)
        {
            error = $"lineEnd expected {expect.LineEnd}, got {actual.LineEnd}.";
            return false;
        }

        if (expect.ScopeKind is not null && expect.ScopeKind != actual.ScopeKind)
        {
            error = $"scopeKind expected \"{expect.ScopeKind}\", got \"{actual.ScopeKind}\".";
            return false;
        }

        if (expect.ScopeIndex is not null && expect.ScopeIndex != actual.ScopeIndex)
        {
            error = $"scopeIndex expected {expect.ScopeIndex}, got {actual.ScopeIndex}.";
            return false;
        }

        if (expect.XmlPath is not null && expect.XmlPath != actual.XmlPath)
        {
            error = $"xmlPath expected \"{expect.XmlPath}\", got \"{actual.XmlPath}\".";
            return false;
        }

        if (expect.Attr is not null && expect.Attr != actual.Attr)
        {
            error = $"attr expected \"{expect.Attr}\", got \"{actual.Attr}\".";
            return false;
        }

        if (expect.FamilyName is not null && expect.FamilyName != actual.Family)
        {
            error = $"familyName expected \"{expect.FamilyName}\", got \"{actual.Family}\".";
            return false;
        }

        if (expect.Command is not null && expect.Command != actual.Command)
        {
            error = $"command expected \"{expect.Command}\", got \"{actual.Command}\".";
            return false;
        }

        if (expect.Go is not null && expect.Go != actual.Go)
        {
            error = $"go expected \"{expect.Go}\", got \"{actual.Go}\".";
            return false;
        }

        if (expect.TextNeedle is not null && expect.TextNeedle != actual.TextNeedle)
        {
            error = $"textNeedle expected \"{expect.TextNeedle}\", got \"{actual.TextNeedle}\".";
            return false;
        }

        if (expect.TypeKey is not null && expect.TypeKey != actual.TypeKey)
        {
            error = $"typeKey expected \"{expect.TypeKey}\", got \"{actual.TypeKey}\".";
            return false;
        }

        if (expect.NestedAnchor is null)
            return true;

        if (actual.NestedAnchor is null)
            return Fail("nestedAnchor expected.", out error);

        return SpanMatches(expect.NestedAnchor, actual.NestedAnchor, out error);
    }

    static bool Fail(string message, out string error)
    {
        error = message;
        return false;
    }

    static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };
}

public sealed record RelationResolveSpecDocument(
    [property: JsonPropertyName("version")] int Version,
    [property: JsonPropertyName("surface")] string Surface,
    [property: JsonPropertyName("source")] string? Source,
    [property: JsonPropertyName("vectors")] IReadOnlyList<RelationResolveSpecVector> Vectors);

public sealed record RelationResolveSpecVector(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("wire")] string? Wire,
    [property: JsonPropertyName("expect")] RelationResolveSpecExpectation? Expect);

public sealed record RelationResolveSpecExpectation(
    [property: JsonPropertyName("family")] string? Family,
    [property: JsonPropertyName("familyError")] string? FamilyError,
    [property: JsonPropertyName("familyName")] string? FamilyName,
    [property: JsonPropertyName("file")] string? File,
    [property: JsonPropertyName("memberKey")] string? MemberKey,
    [property: JsonPropertyName("lineStart")] int? LineStart,
    [property: JsonPropertyName("lineEnd")] int? LineEnd,
    [property: JsonPropertyName("scopeKind")] string? ScopeKind,
    [property: JsonPropertyName("scopeIndex")] int? ScopeIndex,
    [property: JsonPropertyName("xmlPath")] string? XmlPath,
    [property: JsonPropertyName("attr")] string? Attr,
    [property: JsonPropertyName("command")] string? Command,
    [property: JsonPropertyName("go")] string? Go,
    [property: JsonPropertyName("textNeedle")] string? TextNeedle,
    [property: JsonPropertyName("typeKey")] string? TypeKey,
    [property: JsonPropertyName("nestedAnchor")] RelationResolveSpecExpectation? NestedAnchor);
