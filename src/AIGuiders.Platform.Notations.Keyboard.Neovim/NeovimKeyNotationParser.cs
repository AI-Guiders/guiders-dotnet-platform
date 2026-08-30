#nullable enable
using AIGuiders.Platform.Notations.Keyboard.Quarry;

namespace AIGuiders.Platform.Notations.Keyboard;

/// <summary>Neovim key-notation wire parser (GUIDERS-ADR-0021).</summary>
public static class NeovimKeyNotationParser
{
    public static QuarryParseResult Parse(string? input) =>
        NeovimKeyDialect.Instance.Parse(input);

    public static bool TryParseToNormalized(string? input, out NormalizedKeySequence? sequence, out string error) =>
        NeovimKeyDialect.Instance.TryParseToNormalized(input, out sequence, out error);
}
