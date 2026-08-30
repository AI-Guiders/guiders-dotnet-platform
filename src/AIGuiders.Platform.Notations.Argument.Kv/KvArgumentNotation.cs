using AIGuiders.Platform.Notations;
using AIGuiders.Platform.Notations.Argument;

namespace AIGuiders.Platform.Notations.Argument.Kv;

public static class KvArgumentNotation
{
    public static NormalizedArguments Parse(string tail)
    {
        if (string.IsNullOrWhiteSpace(tail))
            return NormalizedArguments.FromRaw("");

        var slots = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var token in tail.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (NotationKvPair.TrySplitFirst(token, '=', out var kv, out _))
                slots[kv.Key] = kv.Value;
        }

        return slots.Count > 0
            ? NormalizedArguments.FromSlots(slots)
            : NormalizedArguments.FromRaw(tail);
    }
}
