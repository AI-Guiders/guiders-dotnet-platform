namespace AIGuiders.Platform.Notations.Argument;

/// <summary>
/// Catalog → parser binding: which wire alphabet and slot schema apply (GUIDERS-ADR-0021).
/// Mirrors <c>BracketNotationProfile</c> for the Argument branch.
/// </summary>
public sealed record ArgumentNotationProfile(
    string? WireClass = null,
    IReadOnlyList<ArgumentSlot>? Slots = null)
{
    public bool IsEmpty =>
        string.IsNullOrWhiteSpace(WireClass) && (Slots is null || Slots.Count == 0);

    public static ArgumentNotationProfile? Merge(ArgumentNotationProfile? existing, ArgumentNotationProfile? incoming)
    {
        if (incoming is null || incoming.IsEmpty)
            return existing;

        if (existing is null || existing.IsEmpty)
            return incoming;

        return new ArgumentNotationProfile(
            incoming.WireClass ?? existing.WireClass,
            incoming.Slots is { Count: > 0 } ? incoming.Slots : existing.Slots);
    }
}
