namespace AIGuiders.Platform.Notations.Argument.Kv;

public static class KvArgumentNotation
{
    public static NormalizedArgTail Parse(string tail)
    {
        if (string.IsNullOrWhiteSpace(tail))
            return NormalizedArgTail.FromRaw("");

        var slots = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var token in tail.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            var eq = token.IndexOf('=');
            if (eq <= 0 || eq >= token.Length - 1)
                continue;

            var key = token[..eq];
            var value = token[(eq + 1)..];
            slots[key] = value;
        }

        return slots.Count > 0
            ? NormalizedArgTail.FromSlots(slots)
            : NormalizedArgTail.FromRaw(tail);
    }
}
