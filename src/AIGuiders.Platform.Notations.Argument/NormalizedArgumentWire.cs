namespace AIGuiders.Platform.Notations.Argument;

/// <summary>
/// Neutral argument/param wire after notation parse (GUIDERS-ADR-0021).
/// Not "tail" — invocation args regardless of slash vs console surface.
/// </summary>
public sealed record NormalizedArgumentWire(
    string? Raw,
    IReadOnlyDictionary<string, string>? Slots,
    string? WireClass = null)
{
    public static NormalizedArgumentWire FromRaw(string? raw, string? wireClass = null) => new(raw, null, wireClass);

    public static NormalizedArgumentWire FromSlots(IReadOnlyDictionary<string, string> slots, string? wireClass = null) =>
        new(null, slots, wireClass);
}
