#nullable enable
using System.Text.Json;
using System.Text.Json.Serialization;
using AIGuiders.Platform.Notations.Keyboard;

namespace AIGuiders.Platform.Notations.Keyboard.Quarry;

public sealed record QuarrySpecDocument(
    [property: JsonPropertyName("version")] int Version,
    [property: JsonPropertyName("surface")] string Surface,
    [property: JsonPropertyName("tier")] string? Tier,
    [property: JsonPropertyName("source")] string? Source,
    [property: JsonPropertyName("vectors")] IReadOnlyList<QuarrySpecVector> Vectors);

public sealed record QuarrySpecVector(
    [property: JsonPropertyName("wire")] string Wire,
    [property: JsonPropertyName("chords")] IReadOnlyList<QuarrySpecChord>? Chords,
    [property: JsonPropertyName("plain")] IReadOnlyList<string>? Plain);

public sealed record QuarrySpecChord(
    [property: JsonPropertyName("mods")] string Mods,
    [property: JsonPropertyName("key")] string Key);

public static class QuarrySpecLoader
{
    public static QuarrySpecDocument Load(string json) =>
        JsonSerializer.Deserialize<QuarrySpecDocument>(json, JsonOptions)
        ?? throw new InvalidOperationException("Quarry spec JSON deserialized to null.");

    static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };
}

public static class QuarrySpecConformance
{
    public static IReadOnlyList<string> ValidateDocument(IKeyboardNotationReader reader, QuarrySpecDocument spec)
    {
        var errors = new List<string>();
        foreach (var vector in spec.Vectors)
        {
            if (!TryValidateVector(reader, vector, out var error))
                errors.Add(error);
        }

        return errors;
    }

    public static bool TryValidateVector(IKeyboardNotationReader reader, QuarrySpecVector vector, out string error)
    {
        error = "";
        if (!reader.TryParseToNormalized(vector.Wire, out var sequence, out var parseError))
        {
            error = $"wire \"{vector.Wire}\": {parseError}";
            return false;
        }

        if (sequence is null)
        {
            error = $"wire \"{vector.Wire}\": parser returned null sequence.";
            return false;
        }

        var expected = BuildExpectedSteps(vector);
        if (expected.Count != sequence.Steps.Count)
        {
            error = $"wire \"{vector.Wire}\": expected {expected.Count} steps, got {sequence.Steps.Count}.";
            return false;
        }

        for (var i = 0; i < expected.Count; i++)
        {
            if (!QuarryIrComparer.StepsEqual(expected[i], sequence.Steps[i], out var stepError))
            {
                error = $"wire \"{vector.Wire}\" step {i}: {stepError}";
                return false;
            }
        }

        return true;
    }

    static List<NormalizedSequenceStep> BuildExpectedSteps(QuarrySpecVector vector)
    {
        var steps = new List<NormalizedSequenceStep>();

        if (vector.Chords is { Count: > 0 })
        {
            foreach (var ch in vector.Chords)
                steps.Add(new NormalizedChordStep(ParseMods(ch.Mods), ch.Key));
        }

        if (vector.Plain is { Count: > 0 })
        {
            foreach (var p in vector.Plain)
                steps.Add(new NormalizedPlainKeyStep(p));
        }

        return steps;
    }

    public static ChordModifierKeys ParseMods(string mods) => QuarryOracleIrMapper.ParseMods(mods);
}
