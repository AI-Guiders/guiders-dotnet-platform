#nullable enable
using System.Text.Json;
using AIGuiders.Platform.Notations.Argument.Kv;
using AIGuiders.Platform.Notations.Command.Console;
using AIGuiders.Platform.Notations.Command.Slash;

namespace AIGuiders.Platform.Notations.Conformance;

public static class NotationSpecConformance
{
    public static NotationSpecDocument Load(string json) =>
        JsonSerializer.Deserialize<NotationSpecDocument>(json, JsonOptions)
        ?? throw new InvalidOperationException("Notation spec JSON deserialized to null.");

    public static IReadOnlyList<string> ValidateDocument(NotationSpecDocument spec)
    {
        var errors = new List<string>();
        foreach (var vector in spec.Vectors)
        {
            if (!TryValidateVector(spec.Surface, vector, out var error))
                errors.Add($"[{vector.Id}] {error}");
        }

        return errors;
    }

    public static bool TryValidateVector(string surface, NotationSpecVector vector, out string error)
    {
        error = "";
        return surface switch
        {
            "command-slash" => TryValidateCommandSlash(vector, out error),
            "argument-kv" => TryValidateArgumentKv(vector, out error),
            "invocation-parity" => TryValidateInvocationParity(vector, out error),
            _ => Fail($"unknown surface \"{surface}\".", out error),
        };
    }

    static bool TryValidateCommandSlash(NotationSpecVector vector, out string error)
    {
        error = "";
        if (vector.Body is null)
            return Fail("body is required.", out error);

        var wire = SlashCommandNotation.ParseBody(vector.Body);
        var expect = vector.Expect;

        if (expect.Tokens is not null
            && !wire.Tokens.SequenceEqual(expect.Tokens, StringComparer.Ordinal))
        {
            error = $"tokens expected [{string.Join(", ", expect.Tokens)}], got [{string.Join(", ", wire.Tokens)}].";
            return false;
        }

        if (expect.EndsWithSpace is not null && expect.EndsWithSpace != wire.EndsWithSpaceAfterTokens)
        {
            error = $"endsWithSpace expected {expect.EndsWithSpace}, got {wire.EndsWithSpaceAfterTokens}.";
            return false;
        }

        return true;
    }

    static bool TryValidateArgumentKv(NotationSpecVector vector, out string error)
    {
        error = "";
        if (vector.Tail is null)
            return Fail("tail is required.", out error);

        var actual = KvArgumentNotation.Parse(vector.Tail);
        var expect = vector.Expect.Slots;
        if (expect is null)
            return Fail("expect.slots is required.", out error);

        if (actual.Slots is null)
        {
            error = "parser returned no slots.";
            return false;
        }

        foreach (var (key, value) in expect)
        {
            if (!actual.Slots.TryGetValue(key, out var got) || got != value)
            {
                error = $"slot {key} expected \"{value}\", got \"{got}\".";
                return false;
            }
        }

        return true;
    }

    static bool TryValidateInvocationParity(NotationSpecVector vector, out string error)
    {
        error = "";
        if (vector.SlashLine is null || vector.ConsoleLine is null)
            return Fail("slashLine and consoleLine are required.", out error);

        if (!SlashCommandNotation.TryParseLine(vector.SlashLine, out var slashWire))
            return Fail($"invalid slashLine \"{vector.SlashLine}\".", out error);

        if (!ConsoleCommandNotation.TryParse(vector.ConsoleLine, out var consoleWire, out _))
            return Fail($"invalid consoleLine \"{vector.ConsoleLine}\".", out error);

        var slashPath = InvocationNotation.FromPathSegments(slashWire.Tokens);
        var consolePath = InvocationNotation.FromPathSegments(consoleWire.Tokens);

        if (!InvocationNotation.PathsEqual(slashPath, consolePath))
        {
            error = $"paths differ: slash \"{slashPath.CanonicalPath}\" vs console \"{consolePath.CanonicalPath}\".";
            return false;
        }

        if (vector.Expect.CanonicalPath is not null
            && !string.Equals(vector.Expect.CanonicalPath, slashPath.CanonicalPath, StringComparison.OrdinalIgnoreCase))
        {
            error = $"canonicalPath expected \"{vector.Expect.CanonicalPath}\", got \"{slashPath.CanonicalPath}\".";
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
