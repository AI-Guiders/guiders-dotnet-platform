namespace AIGuiders.Platform.Notations.Argument;

/// <summary>Neutral invocation args after notation parse (GUIDERS-ADR-0021).</summary>
public sealed record NormalizedArguments(
    string? Raw,
    IReadOnlyDictionary<string, string>? Slots,
    string? WireClass = null)
{
    public static NormalizedArguments FromRaw(string? raw, string? wireClass = null) => new(raw, null, wireClass);

    public static NormalizedArguments FromSlots(IReadOnlyDictionary<string, string> slots, string? wireClass = null) =>
        new(null, slots, wireClass);
}
