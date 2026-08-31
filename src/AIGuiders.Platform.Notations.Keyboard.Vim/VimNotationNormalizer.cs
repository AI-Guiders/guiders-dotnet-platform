using AIGuiders.Platform.IntermediateRepresentation.Keyboard;
#nullable enable

namespace AIGuiders.Platform.Notations.Keyboard;

static class VimNotationNormalizer
{
    public static NormalizedKeySequence FromVimSteps(IReadOnlyList<VimNotationStep> steps)
    {
        if (steps.Count == 0)
            return NormalizedKeySequence.Empty;

        var list = new List<NormalizedSequenceStep>(steps.Count);
        foreach (var s in steps)
        {
            switch (s)
            {
                case VimNotationChordStep ch:
                    list.Add(new NormalizedChordStep(MapVimPrefixes(ch.ModifierPrefixes), ChordSemanticNormalizer.NormalizeKeySymbol(ch.Key)));
                    break;
                case VimNotationPlainStep pl:
                    list.Add(new NormalizedPlainKeyStep(ChordSemanticNormalizer.NormalizeKeySymbol(pl.Token)));
                    break;
            }
        }

        return new NormalizedKeySequence(list);
    }

    static ChordModifierKeys MapVimPrefixes(IReadOnlyList<string> prefixes)
    {
        ChordModifierKeys m = 0;
        foreach (var p in prefixes)
        {
            m |= p switch
            {
                "C-" => ChordModifierKeys.Control,
                "M-" or "A-" or "Alt-" => ChordModifierKeys.Alt,
                "S-" => ChordModifierKeys.Shift,
                "D-" => ChordModifierKeys.Meta,
                _ => 0,
            };
        }

        return m;
    }
}
