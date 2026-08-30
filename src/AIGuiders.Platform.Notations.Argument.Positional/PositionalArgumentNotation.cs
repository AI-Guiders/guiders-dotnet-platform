namespace AIGuiders.Platform.Notations.Argument.Positional;

public static class PositionalArgumentNotation
{
    public const string WireClassPositional = "positional";

    public static NormalizedArgTail Parse(string tail)
    {
        if (string.IsNullOrWhiteSpace(tail))
            return NormalizedArgTail.FromRaw("", WireClassPositional);

        var parts = tail.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
            return NormalizedArgTail.FromRaw(tail.Trim(), WireClassPositional);

        var slots = new Dictionary<string, string>(parts.Length, StringComparer.Ordinal);
        for (var i = 0; i < parts.Length; i++)
            slots[i.ToString()] = parts[i];

        return NormalizedArgTail.FromSlots(slots, WireClassPositional);
    }
}
