namespace AIGuiders.Platform.Notations;

public enum InvocationArgParameterKind
{
    Flag,
    Value,
    Positional,
}

/// <summary>Federation arg slot schema (catalog / capabilities). Interpretation is per commandId.</summary>
public sealed record InvocationArgParameter(
    string Name,
    InvocationArgParameterKind Kind = InvocationArgParameterKind.Value,
    string? LongOption = null,
    string? ShortOption = null);

/// <summary>How to parse invocation tail wire for a resolved command.</summary>
public sealed record InvocationArgDescriptor(
    string? TailWireClass = null,
    IReadOnlyList<InvocationArgParameter>? Parameters = null);

public static class InvocationArgWireClasses
{
    public const string Kv = "kv";
    public const string Cli = "cli";
    public const string Positional = "positional";
    public const string Delimited = "delimited";
    public const string Colon = "colon";
    public const string Raw = "raw";
}
