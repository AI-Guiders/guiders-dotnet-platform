#nullable enable

namespace AIGuiders.Platform.Notations;

/// <summary>Legacy alias for <see cref="Argument.NormalizedArgumentWire"/> (GUIDERS-ADR-0021 Wave 4b).</summary>
[Obsolete("Use AIGuiders.Platform.Notations.Argument.NormalizedArgumentWire — ArgTail was slash jargon.")]
public sealed record NormalizedArgTail(
    string? Raw,
    IReadOnlyDictionary<string, string>? Slots,
    string? WireClass = null)
{
    public static NormalizedArgTail FromRaw(string? raw, string? wireClass = null) => new(raw, null, wireClass);

    public static NormalizedArgTail FromSlots(IReadOnlyDictionary<string, string> slots, string? wireClass = null) =>
        new(null, slots, wireClass);

    public static implicit operator Argument.NormalizedArgumentWire(NormalizedArgTail tail) =>
        new(tail.Raw, tail.Slots, tail.WireClass);

    public static implicit operator NormalizedArgTail(Argument.NormalizedArgumentWire wire) =>
        new(wire.Raw, wire.Slots, wire.WireClass);
}
