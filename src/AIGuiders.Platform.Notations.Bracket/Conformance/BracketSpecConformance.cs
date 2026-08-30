#nullable enable
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIGuiders.Platform.Notations.Bracket.Conformance;

public static class BracketSpecConformance
{
    public static BracketSpecDocument Load(string json) =>
        JsonSerializer.Deserialize<BracketSpecDocument>(json, JsonOptions)
        ?? throw new InvalidOperationException("Bracket spec JSON deserialized to null.");

    public static IReadOnlyList<string> ValidateDocument(BracketSpecDocument spec)
    {
        var errors = new List<string>();
        var profile = BracketProfiles.CdpSquareKeyValue;
        foreach (var vector in spec.Vectors)
        {
            if (!TryValidateVector(vector, profile, out var error))
                errors.Add($"[{vector.Id}] {error}");
        }

        return errors;
    }

    public static bool TryValidateVector(
        BracketSpecVector vector,
        BracketNotationProfile profile,
        out string error)
    {
        error = "";
        if (vector.Wire is null)
            return Fail("wire is required.", out error);

        if (!BracketReader.Default.TryRead(vector.Wire, profile, out var actual, out error) || actual is null)
            return false;

        if (vector.Expect.Axes is null)
            return Fail("expect.axes is required.", out error);

        if (actual.Axes.Count != vector.Expect.Axes.Count)
        {
            error = $"axis count expected {vector.Expect.Axes.Count}, got {actual.Axes.Count}.";
            return false;
        }

        for (var i = 0; i < vector.Expect.Axes.Count; i++)
        {
            if (!AxisMatches(vector.Expect.Axes[i], actual.Axes[i], profile, out error))
                return false;
        }

        return true;
    }

    static bool AxisMatches(
        BracketSpecAxis expect,
        BracketAxis actual,
        BracketNotationProfile profile,
        out string error)
    {
        error = "";
        if (!string.Equals(expect.Key, actual.Key, StringComparison.OrdinalIgnoreCase))
        {
            error = $"key expected \"{expect.Key}\", got \"{actual.Key}\".";
            return false;
        }

        if (expect.Value is not null && expect.Value != actual.Value)
        {
            error = $"value expected \"{expect.Value}\", got \"{actual.Value}\".";
            return false;
        }

        if (expect.ValueWireClass is not null
            && !string.Equals(expect.ValueWireClass, actual.ValueWireClass, StringComparison.Ordinal))
        {
            error = $"valueWireClass expected \"{expect.ValueWireClass}\", got \"{actual.ValueWireClass}\".";
            return false;
        }

        if (expect.NestedAxes is null)
            return true;

        if (actual.Nested is null)
            return Fail("nested axes expected.", out error);

        if (actual.Nested.Axes.Count != expect.NestedAxes.Count)
        {
            error = $"nested axis count expected {expect.NestedAxes.Count}, got {actual.Nested.Axes.Count}.";
            return false;
        }

        for (var i = 0; i < expect.NestedAxes.Count; i++)
        {
            if (!AxisMatches(expect.NestedAxes[i], actual.Nested.Axes[i], profile, out error))
                return false;
        }

        return true;
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

public sealed record BracketSpecDocument(
    [property: JsonPropertyName("version")] int Version,
    [property: JsonPropertyName("surface")] string Surface,
    [property: JsonPropertyName("source")] string? Source,
    [property: JsonPropertyName("vectors")] IReadOnlyList<BracketSpecVector> Vectors);

public sealed record BracketSpecVector(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("wire")] string? Wire,
    [property: JsonPropertyName("expect")] BracketSpecExpectation Expect);

public sealed record BracketSpecExpectation(
    [property: JsonPropertyName("axes")] IReadOnlyList<BracketSpecAxis>? Axes);

public sealed record BracketSpecAxis(
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("value")] string? Value,
    [property: JsonPropertyName("valueWireClass")] string? ValueWireClass,
    [property: JsonPropertyName("nestedAxes")] IReadOnlyList<BracketSpecAxis>? NestedAxes);
