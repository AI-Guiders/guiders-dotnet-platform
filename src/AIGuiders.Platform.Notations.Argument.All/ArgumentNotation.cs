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
            return NormalizedArguments.FromRaw("", profile?.WireClass);

        var wireClass = ResolveWireClass(wire, profile);
        return wireClass switch
        {
            ArgumentWireClasses.Kv => KvArgumentNotation.Parse(wire),
            ArgumentWireClasses.Cli when profile?.Slots is { Count: > 0 } =>
                CliArgumentNotation.ParseWithSchema(wire, profile.Slots),
            ArgumentWireClasses.Cli => CliArgumentNotation.Parse(wire),
            ArgumentWireClasses.Positional => PositionalArgumentNotation.Parse(wire),
            ArgumentWireClasses.Delimited or ArgumentWireClasses.Colon =>
                DelimitedArgumentNotation.Parse(wire),
            _ => NormalizedArguments.FromRaw(wire.Trim(), wireClass),
        };
    }

    public static string ResolveWireClass(string wire, ArgumentNotationProfile? profile)
    {
        if (!string.IsNullOrWhiteSpace(profile?.WireClass))
            return profile.WireClass!;

        if (wire.Contains('=') && !wire.TrimStart().StartsWith('-'))
            return ArgumentWireClasses.Kv;

        if (wire.TrimStart().StartsWith('-'))
            return ArgumentWireClasses.Cli;

        if (wire.Contains(':'))
            return ArgumentWireClasses.Colon;

        return ArgumentWireClasses.Raw;
    }
}
