#nullable enable
using AIGuiders.Platform.Notations.Keyboard.Quarry;

namespace AIGuiders.Platform.Notations.Keyboard;

/// <summary>Emacs <c>kbd</c> wire parser (GUIDERS-ADR-0021).</summary>
public static class EmacsKbdNotationParser
{
    public static QuarryParseResult Parse(string? input) =>
        EmacsKbdDialect.Instance.Parse(input);

    public static bool TryParseToNormalized(string? input, out NormalizedKeySequence? sequence, out string error) =>
        EmacsKbdDialect.Instance.TryParseToNormalized(input, out sequence, out error);
}
