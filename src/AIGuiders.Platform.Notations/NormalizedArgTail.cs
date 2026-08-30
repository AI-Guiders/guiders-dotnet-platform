namespace AIGuiders.Platform.Notations;

public sealed record NormalizedArgTail(
    string? Raw,
    IReadOnlyDictionary<string, string>? Slots,
    string? WireClass = null)
{
    public static NormalizedArgTail FromRaw(string? raw, string? wireClass = null) => new(raw, null, wireClass);

    public static NormalizedArgTail FromSlots(IReadOnlyDictionary<string, string> slots, string? wireClass = null) =>
        new(null, slots, wireClass);
}
