namespace AIGuiders.Platform.Abstractions;

/// <summary>Pulse truncation defaults — aligned with CDP inventory observe limits.</summary>
public static class PulseFormat
{
    public const int DefaultMaxChars = 240;
    public const int InventoryObserveMaxChars = 480;

    public static string? Truncate(string? value, int maxChars = DefaultMaxChars)
    {
        if (string.IsNullOrEmpty(value))
            return value;
        if (value.Length <= maxChars)
            return value;
        return value[..maxChars] + "…";
    }

    public static string JoinBits(IEnumerable<string?> bits, int maxChars = DefaultMaxChars)
    {
        var parts = bits.Where(static s => !string.IsNullOrWhiteSpace(s)).Select(static s => s!.Trim());
        return Truncate(string.Join(' ', parts), maxChars) ?? "";
    }
}
