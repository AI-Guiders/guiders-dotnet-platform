namespace AIGuiders.Platform.Notations.Argument.Delimited;

public static class DelimitedArgumentNotation
{
    public const string WireClassColon = "colon";

    public static NormalizedArgTail Parse(string tail, char delimiter = ':')
    {
        if (string.IsNullOrWhiteSpace(tail))
            return NormalizedArgTail.FromRaw("", WireClassColon);

        var parts = tail.Split(delimiter, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return NormalizedArgTail.FromRaw(tail.Trim(), WireClassColon);

        var slots = new Dictionary<string, string>(parts.Length, StringComparer.Ordinal);
        for (var i = 0; i < parts.Length; i++)
            slots[i.ToString()] = parts[i];

        return NormalizedArgTail.FromSlots(slots, WireClassColon);
    }
}
