#nullable enable

namespace AIGuiders.Platform.InputNotation.Quarry;

public static class QuarryWireNormalizer
{
    public static NormalizedKeySequence ToNormalized(
        IReadOnlyList<QuarryWireStep> steps,
        Func<string, ChordModifierKeys> mapModifier,
        Func<string, string> normalizeKey)
    {
        if (steps.Count == 0)
            return NormalizedKeySequence.Empty;

        var list = new List<NormalizedSequenceStep>(steps.Count);
        foreach (var s in steps)
        {
            switch (s)
            {
                case QuarryWireChordStep ch:
                    list.Add(new NormalizedChordStep(MapPrefixes(ch.ModifierPrefixes, mapModifier), normalizeKey(ch.Key)));
                    break;
                case QuarryWirePlainStep pl:
                    list.Add(new NormalizedPlainKeyStep(normalizeKey(pl.Token)));
                    break;
            }
        }

        return new NormalizedKeySequence(list);
    }

    static ChordModifierKeys MapPrefixes(IReadOnlyList<string> prefixes, Func<string, ChordModifierKeys> mapModifier)
    {
        ChordModifierKeys m = 0;
        foreach (var p in prefixes)
            m |= mapModifier(p);

        return m;
    }
}
