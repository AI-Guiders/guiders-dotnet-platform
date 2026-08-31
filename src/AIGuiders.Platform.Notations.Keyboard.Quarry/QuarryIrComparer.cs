using AIGuiders.Platform.IntermediateRepresentation.Keyboard;
#nullable enable
using System.Text;

namespace AIGuiders.Platform.Notations.Keyboard.Quarry;

public static class QuarryIrComparer
{
    public static bool SequencesEqual(NormalizedKeySequence left, NormalizedKeySequence right, out string error)
    {
        error = "";
        if (left.Steps.Count != right.Steps.Count)
        {
            error = $"step count expected {left.Steps.Count}, got {right.Steps.Count}.";
            return false;
        }

        for (var i = 0; i < left.Steps.Count; i++)
        {
            if (!StepsEqual(left.Steps[i], right.Steps[i], out error))
            {
                error = $"step {i}: {error}";
                return false;
            }
        }

        return true;
    }

    public static bool StepsEqual(NormalizedSequenceStep expected, NormalizedSequenceStep actual, out string error)
    {
        error = "";
        switch (expected)
        {
            case NormalizedChordStep expChord when actual is NormalizedChordStep actChord:
                if (expChord.Modifiers != actChord.Modifiers)
                {
                    error = $"modifiers expected {expChord.Modifiers}, got {actChord.Modifiers}.";
                    return false;
                }

                if (expChord.KeySymbol != actChord.KeySymbol)
                {
                    error = $"key expected {expChord.KeySymbol}, got {actChord.KeySymbol}.";
                    return false;
                }

                return true;
            case NormalizedPlainKeyStep expPlain when actual is NormalizedPlainKeyStep actPlain:
                if (expPlain.KeySymbol != actPlain.KeySymbol)
                {
                    error = $"plain key expected {expPlain.KeySymbol}, got {actPlain.KeySymbol}.";
                    return false;
                }

                return true;
            default:
                error = $"step kind mismatch: expected {expected.GetType().Name}, got {actual.GetType().Name}.";
                return false;
        }
    }

    public static string FormatSequence(NormalizedKeySequence sequence)
    {
        if (sequence.Steps.Count == 0)
            return "(empty)";

        var sb = new StringBuilder();
        for (var i = 0; i < sequence.Steps.Count; i++)
        {
            if (i > 0)
                sb.Append(' ');

            sb.Append(FormatStep(sequence.Steps[i]));
        }

        return sb.ToString();
    }

    public static string FormatStep(NormalizedSequenceStep step) =>
        step switch
        {
            NormalizedChordStep chord => $"[{FormatMods(chord.Modifiers)}+{chord.KeySymbol}]",
            NormalizedPlainKeyStep plain => plain.KeySymbol,
            _ => step.ToString() ?? "?",
        };

    static string FormatMods(ChordModifierKeys modifiers)
    {
        if (modifiers == ChordModifierKeys.None)
            return "None";

        var parts = new List<string>(4);
        if (modifiers.HasFlag(ChordModifierKeys.Control))
            parts.Add("Control");
        if (modifiers.HasFlag(ChordModifierKeys.Alt))
            parts.Add("Alt");
        if (modifiers.HasFlag(ChordModifierKeys.Shift))
            parts.Add("Shift");
        if (modifiers.HasFlag(ChordModifierKeys.Meta))
            parts.Add("Meta");

        return string.Join('|', parts);
    }
}
