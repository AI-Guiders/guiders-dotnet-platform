#nullable enable

namespace AIGuiders.Platform.Notations.Keyboard;

/// <summary>Facade over notation surfaces → <see cref="NormalizedKeySequence"/> (GUIDERS-ADR-0021).</summary>
public static class KeyboardNotationParser
{
    public static bool TryParseToSequence(
        string? wire,
        KeyboardNotationSurface surface,
        out NormalizedKeySequence? sequence,
        out string error)
    {
        sequence = null;
        error = "";

        return surface switch
        {
            KeyboardNotationSurface.VimDocument => VimChordNotationParser.TryParseToNormalized(wire, out sequence, out error),
            KeyboardNotationSurface.KeyGestureConfig => KeyGestureChordSyntax.TryParseToNormalized(wire, out sequence, out error),
            KeyboardNotationSurface.NeovimKbd => NeovimKeyNotationParser.TryParseToNormalized(wire, out sequence, out error),
            KeyboardNotationSurface.EmacsKbd => EmacsKbdNotationParser.TryParseToNormalized(wire, out sequence, out error),
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
