using AIGuiders.Platform.Modeling.Notations.Argument;

namespace AIGuiders.Platform.Notations.Argument.Positional;

public static class PositionalArgumentNotation
{
    public const string ReaderIdPositional = "positional";

    public static NormalizedArguments Parse(string tail)
    {
        if (string.IsNullOrWhiteSpace(tail))
            return NormalizedArguments.FromRaw("", ReaderIdPositional);

        var parts = tail.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
            return NormalizedArguments.FromRaw(tail.Trim(), ReaderIdPositional);

        var slots = new Dictionary<string, string>(parts.Length, StringComparer.Ordinal);
        for (var i = 0; i < parts.Length; i++)
            slots[i.ToString()] = parts[i];

        return NormalizedArguments.FromSlots(slots, ReaderIdPositional);
    }
}
