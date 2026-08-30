#nullable enable
using System.Text.Json;
using AIGuiders.Platform.Notations;
using AIGuiders.Platform.Notations.Argument.Cli;
using AIGuiders.Platform.Notations.Argument.Delimited;
using AIGuiders.Platform.Notations.Argument.Kv;
using AIGuiders.Platform.Notations.Argument.Positional;
using AIGuiders.Platform.Notations.Command;
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
            "command-console" => TryValidateCommandConsole(vector, out error),
            "argument-kv" => TryValidateArgumentKv(vector, out error),
            "argument-delimited" => TryValidateArgumentDelimited(vector, out error),
            "argument-positional" => TryValidateArgumentPositional(vector, out error),
            "argument-cli" => TryValidateArgumentCli(vector, out error),
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

    static bool TryValidateCommandConsole(NotationSpecVector vector, out string error)
    {
        error = "";
        if (vector.Line is null)
            return Fail("line is required.", out error);

        if (!ConsoleCommandNotation.TryParse(vector.Line, out var pathWire, out var argTail))
            return Fail($"invalid line \"{vector.Line}\".", out error);

        var expect = vector.Expect;
        if (expect.Tokens is not null
            && !pathWire.Tokens.SequenceEqual(expect.Tokens, StringComparer.Ordinal))
        {
            error = $"tokens expected [{string.Join(", ", expect.Tokens)}], got [{string.Join(", ", pathWire.Tokens)}].";
            return false;
        }

        if (expect.EndsWithSpace is not null && expect.EndsWithSpace != pathWire.EndsWithSpaceAfterTokens)
        {
            error = $"endsWithSpace expected {expect.EndsWithSpace}, got {pathWire.EndsWithSpaceAfterTokens}.";
            return false;
        }

        if (expect.Slots is not null && !SlotsMatch(expect.Slots, argTail, out error))
            return false;

        return true;
    }

    static bool TryValidateArgumentPositional(NotationSpecVector vector, out string error)
    {
        error = "";
        if (vector.Tail is null)
            return Fail("tail is required.", out error);

        var actual = PositionalArgumentNotation.Parse(vector.Tail);
        if (vector.Expect.WireClass is not null
            && !string.Equals(vector.Expect.WireClass, actual.WireClass, StringComparison.Ordinal))
        {
            error = $"wireClass expected \"{vector.Expect.WireClass}\", got \"{actual.WireClass}\".";
            return false;
        }

        if (vector.Expect.Slots is null)
            return Fail("expect.slots is required.", out error);

        return SlotsMatch(vector.Expect.Slots, actual, out error);
    }

    static bool TryValidateArgumentCli(NotationSpecVector vector, out string error)
    {
        error = "";
        if (vector.Tail is null)
            return Fail("tail is required.", out error);

        var actual = CliArgumentNotation.Parse(vector.Tail);
        if (vector.Expect.WireClass is not null
            && !string.Equals(vector.Expect.WireClass, actual.WireClass, StringComparison.Ordinal))
        {
            error = $"wireClass expected \"{vector.Expect.WireClass}\", got \"{actual.WireClass}\".";
            return false;
        }

        if (vector.Expect.Slots is null)
            return Fail("expect.slots is required.", out error);

        return SlotsMatch(vector.Expect.Slots, actual, out error);
    }

    static bool SlotsMatch(IReadOnlyDictionary<string, string> expect, NormalizedArgTail actual, out string error)
    {
        error = "";
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

    static bool TryValidateArgumentKv(NotationSpecVector vector, out string error)
    {
        error = "";
        if (vector.Tail is null)
            return Fail("tail is required.", out error);

        var actual = KvArgumentNotation.Parse(vector.Tail);
        if (vector.Expect.Slots is null)
            return Fail("expect.slots is required.", out error);

        return SlotsMatch(vector.Expect.Slots, actual, out error);
    }

    static bool TryValidateArgumentDelimited(NotationSpecVector vector, out string error)
    {
        error = "";
        if (vector.Tail is null)
            return Fail("tail is required.", out error);

        var actual = DelimitedArgumentNotation.Parse(vector.Tail);
        if (vector.Expect.WireClass is not null
            && !string.Equals(vector.Expect.WireClass, actual.WireClass, StringComparison.Ordinal))
        {
            error = $"wireClass expected \"{vector.Expect.WireClass}\", got \"{actual.WireClass}\".";
            return false;
        }

        var expect = vector.Expect.Slots;
        if (expect is null)
            return Fail("expect.slots is required.", out error);

        return SlotsMatch(expect, actual, out error);
    }

    static bool TryValidateInvocationParity(NotationSpecVector vector, out string error)
    {
        error = "";
        if (vector.SlashLine is null || vector.ConsoleLine is null)
            return Fail("slashLine and consoleLine are required.", out error);

        if (!SlashCommandNotation.TryParseLine(vector.SlashLine, out var slashWire))
            return Fail($"invalid slashLine \"{vector.SlashLine}\".", out error);

        if (!ConsoleCommandNotation.TryParse(vector.ConsoleLine, out var consoleWire, out var consoleArgs))
            return Fail($"invalid consoleLine \"{vector.ConsoleLine}\".", out error);

        var slashPath = global::AIGuiders.Platform.Notations.Command.InvocationNotation.FromPathSegments(slashWire.Tokens);
        var consolePath = global::AIGuiders.Platform.Notations.Command.InvocationNotation.FromPathSegments(consoleWire.Tokens);

        if (!global::AIGuiders.Platform.Notations.Command.InvocationNotation.PathsEqual(slashPath, consolePath))
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

        if (vector.Expect.Slots is not null && !SlotsMatch(vector.Expect.Slots, consoleArgs, out error))
            return false;

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
