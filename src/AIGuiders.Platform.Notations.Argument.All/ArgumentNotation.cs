using AIGuiders.Platform.Notations.Argument.Cli;
using AIGuiders.Platform.Notations.Argument.Delimited;
using AIGuiders.Platform.Notations.Argument.Kv;
using AIGuiders.Platform.Notations.Argument.Positional;

namespace AIGuiders.Platform.Notations.Argument;

/// <summary>Profile-driven argument wire parse — SSOT for catalog → IR (GUIDERS-ADR-0021).</summary>
public static class ArgumentNotation
{
    public static NormalizedArguments Parse(string? wire, ArgumentNotationProfile? profile = null)
    {
        if (string.IsNullOrWhiteSpace(wire))
            return NormalizedArguments.FromRaw("", profile?.ReaderId);

        var readerId = ResolveReaderId(wire, profile);
        return readerId switch
        {
            ArgumentReaders.Kv => KvArgumentNotation.Parse(wire),
            ArgumentReaders.Cli when profile?.Slots is { Count: > 0 } =>
                CliArgumentNotation.ParseWithSchema(wire, profile.Slots),
            ArgumentReaders.Cli => CliArgumentNotation.Parse(wire),
            ArgumentReaders.Positional => PositionalArgumentNotation.Parse(wire),
            ArgumentReaders.Delimited or ArgumentReaders.Colon =>
                DelimitedArgumentNotation.Parse(wire),
            _ => NormalizedArguments.FromRaw(wire.Trim(), readerId),
        };
    }

    public static string ResolveReaderId(string wire, ArgumentNotationProfile? profile)
    {
        if (!string.IsNullOrWhiteSpace(profile?.ReaderId))
            return profile.ReaderId!;

        if (wire.Contains('=') && !wire.TrimStart().StartsWith('-'))
            return ArgumentReaders.Kv;

        if (wire.TrimStart().StartsWith('-'))
            return ArgumentReaders.Cli;

        if (wire.Contains(':'))
            return ArgumentReaders.Colon;

        return ArgumentReaders.Raw;
    }
}
