namespace AIGuiders.Platform.Notations;

public sealed record NormalizedArgTail(
    string? Raw,
    IReadOnlyDictionary<string, string>? Slots)
{
    public static NormalizedArgTail FromRaw(string? raw) => new(raw, null);

    public static NormalizedArgTail FromSlots(IReadOnlyDictionary<string, string> slots) =>
        new(null, slots);
}
