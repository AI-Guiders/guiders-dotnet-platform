namespace AIGuiders.Platform.IntermediateRepresentation.Argument;

/// <summary>Neutral invocation args after notation parse (GUIDERS-ADR-0021).</summary>
public sealed record NormalizedArguments(
    string? Raw,
    IReadOnlyDictionary<string, string>? Slots,
    string? ReaderId = null)
{
    public static NormalizedArguments FromRaw(string? raw, string? readerId = null) => new(raw, null, readerId);

    public static NormalizedArguments FromSlots(IReadOnlyDictionary<string, string> slots, string? readerId = null) =>
        new(null, slots, readerId);
}
