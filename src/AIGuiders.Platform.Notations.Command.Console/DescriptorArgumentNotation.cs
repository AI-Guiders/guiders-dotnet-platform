using AIGuiders.Platform.Notations;
using AIGuiders.Platform.Notations.Argument.Cli;
using AIGuiders.Platform.Notations.Argument.Delimited;
using AIGuiders.Platform.Notations.Argument.Kv;
using AIGuiders.Platform.Notations.Argument.Positional;

namespace AIGuiders.Platform.Notations.Command.Console;

/// <summary>Descriptor-driven tail parse: wireClass + per-command arg schema (GUIDERS-ADR-0021).</summary>
public static class DescriptorArgumentNotation
{
    public static NormalizedArgTail ParseTail(string? tail, InvocationArgDescriptor? descriptor = null)
    {
        if (string.IsNullOrWhiteSpace(tail))
            return NormalizedArgTail.FromRaw("", descriptor?.TailWireClass);

        var wireClass = ResolveWireClass(tail, descriptor);
        return wireClass switch
        {
            InvocationArgWireClasses.Kv => KvArgumentNotation.Parse(tail),
            InvocationArgWireClasses.Cli when descriptor?.Parameters is { Count: > 0 } =>
                CliArgumentNotation.ParseWithSchema(tail, descriptor.Parameters),
            InvocationArgWireClasses.Cli => CliArgumentNotation.Parse(tail),
            InvocationArgWireClasses.Positional => PositionalArgumentNotation.Parse(tail),
            InvocationArgWireClasses.Delimited or InvocationArgWireClasses.Colon =>
                DelimitedArgumentNotation.Parse(tail),
            _ => NormalizedArgTail.FromRaw(tail.Trim(), wireClass),
        };
    }

    static string ResolveWireClass(string tail, InvocationArgDescriptor? descriptor)
    {
        if (!string.IsNullOrWhiteSpace(descriptor?.TailWireClass))
            return descriptor.TailWireClass!;

        if (tail.Contains('=') && !tail.TrimStart().StartsWith('-'))
            return InvocationArgWireClasses.Kv;

        if (tail.TrimStart().StartsWith('-'))
            return InvocationArgWireClasses.Cli;

        if (tail.Contains(':'))
            return InvocationArgWireClasses.Colon;

        return InvocationArgWireClasses.Raw;
    }
}
