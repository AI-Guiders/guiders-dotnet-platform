#nullable enable

namespace AIGuiders.Platform.InputNotation;

/// <summary>Vim-doc notation reader (GUIDERS-ADR-0016).</summary>
public sealed class VimNotationReader : IInputNotationReader
{
    public static VimNotationReader Instance { get; } = new();

    public string SurfaceId => "vim";

    public bool TryParseToNormalized(string? wire, out NormalizedKeySequence? sequence, out string error) =>
        VimChordNotationParser.TryParseToNormalized(wire, out sequence, out error);
}
