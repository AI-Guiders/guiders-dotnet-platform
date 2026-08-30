using AIGuiders.Platform.Notations.Argument;

namespace AIGuiders.Platform.Notations.Argument.Delimited;

public static class DelimitedArgumentNotation
{
    public const string ReaderIdColon = "colon";

    public static NormalizedArguments Parse(string tail, char delimiter = ':')
    {
        if (string.IsNullOrWhiteSpace(tail))
            return NormalizedArguments.FromRaw("", ReaderIdColon);

        var parts = tail.Split(delimiter, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return NormalizedArguments.FromRaw(tail.Trim(), ReaderIdColon);

        var slots = new Dictionary<string, string>(parts.Length, StringComparer.Ordinal);
        for (var i = 0; i < parts.Length; i++)
            slots[i.ToString()] = parts[i];

        return NormalizedArguments.FromSlots(slots, ReaderIdColon);
    }
}
