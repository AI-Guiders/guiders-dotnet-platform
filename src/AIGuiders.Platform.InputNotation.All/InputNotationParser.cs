#nullable enable

namespace AIGuiders.Platform.InputNotation;

/// <summary>Facade over notation surfaces → <see cref="NormalizedKeySequence"/> (GUIDERS-ADR-0016).</summary>
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
            InputNotationSurface.NeovimKbd => NeovimKeyNotationParser.TryParseToNormalized(wire, out sequence, out error),
            InputNotationSurface.EmacsKbd => EmacsKbdNotationParser.TryParseToNormalized(wire, out sequence, out error),
            _ => Fail($"Unknown notation surface: {surface}", out sequence, out error),
        };
    }

    static bool Fail(string message, out NormalizedKeySequence? sequence, out string error)
    {
        sequence = null;
        error = message;
        return false;
    }
}
