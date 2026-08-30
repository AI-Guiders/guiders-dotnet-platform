using AIGuiders.Platform.Notations.Argument;

namespace AIGuiders.Platform.Notations.Argument.Positional;

public static class PositionalArgumentNotation
{
    public const string WireClassPositional = "positional";

    public static NormalizedArgumentWire Parse(string tail)
    {
        if (string.IsNullOrWhiteSpace(tail))
            return NormalizedArgumentWire.FromRaw("", WireClassPositional);

        var parts = tail.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
            return NormalizedArgumentWire.FromRaw(tail.Trim(), WireClassPositional);

        var slots = new Dictionary<string, string>(parts.Length, StringComparer.Ordinal);
        for (var i = 0; i < parts.Length; i++)
            slots[i.ToString()] = parts[i];

        return NormalizedArgumentWire.FromSlots(slots, WireClassPositional);
    }
}
