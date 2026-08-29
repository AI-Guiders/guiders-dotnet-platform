#nullable enable
using AIGuiders.Platform.InputNotation.Quarry;

namespace AIGuiders.Platform.InputNotation;

/// <summary>Neovim key-notation wire parser (GUIDERS-ADR-0016).</summary>
public static class NeovimKeyNotationParser
{
    public static QuarryParseResult Parse(string? input) =>
        NeovimKeyDialect.Instance.Parse(input);

    public static bool TryParseToNormalized(string? input, out NormalizedKeySequence? sequence, out string error) =>
        NeovimKeyDialect.Instance.TryParseToNormalized(input, out sequence, out error);
}
