#nullable enable

namespace AIGuiders.Platform.InputNotation;

/// <summary>Facade over notation surfaces → <see cref="NormalizedKeySequence"/> (GUIDERS-ADR-0015 quarry).</summary>
public static class InputNotationParser
{
    public static bool TryParseToSequence(
        string? wire,
        InputNotationSurface surface,
        out NormalizedKeySequence? sequence,
        out string error)
    {
        sequence = null;
        error = "";

        return surface switch
        {
            InputNotationSurface.VimDocument => VimChordNotationParser.TryParseToNormalized(wire, out sequence, out error),
            InputNotationSurface.KeyGestureConfig => KeyGestureChordSyntax.TryParseToNormalized(wire, out sequence, out error),
            InputNotationSurface.MelodyNote => TryParseMelodyNoteSequence(wire, out sequence, out error),
            _ => Fail($"Unknown notation surface: {surface}", out sequence, out error),
        };
    }

    static bool TryParseMelodyNoteSequence(string? wire, out NormalizedKeySequence? sequence, out string error)
    {
        if (!TryParseMelodyNoteStep(wire, out var step, out error))
        {
            sequence = null;
            return false;
        }

        sequence = new NormalizedKeySequence([step!]);
        error = "";
        return true;
    }

    public static bool TryParseMelodyNoteStep(string? wire, out NormalizedSequenceStep? step, out string error)
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
    
    static bool Fail(string message, out NormalizedKeySequence? sequence, out string error)
    {
        sequence = null;
        error = message;
        return false;
    }
}
