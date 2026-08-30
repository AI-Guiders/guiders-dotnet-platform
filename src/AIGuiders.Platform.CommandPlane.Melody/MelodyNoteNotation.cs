#nullable enable
using AIGuiders.Platform.Notations.Keyboard;

namespace AIGuiders.Platform.CommandPlane.Melody;

/// <summary>Melody note-token wire (plain key slug step).</summary>
public static class MelodyNoteNotation
{
    public static bool TryParseStep(string? wire, out NormalizedSequenceStep? step, out string error)
    {
        step = null;
        error = "";
        if (string.IsNullOrWhiteSpace(wire))
        {
            error = "Melody note wire is required.";
            return false;
        }

        step = new NormalizedPlainKeyStep(ChordSemanticNormalizer.NormalizeKeySymbol(wire.Trim()));
        return true;
    }
}
