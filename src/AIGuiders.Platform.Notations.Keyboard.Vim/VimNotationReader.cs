using AIGuiders.Platform.Modeling.Notations.Keyboard;
#nullable enable

namespace AIGuiders.Platform.Notations.Keyboard;

/// <summary>Vim-doc notation reader (GUIDERS-ADR-0021).</summary>
public sealed class VimNotationReader : IKeyboardNotationReader
{
    public static VimNotationReader Instance { get; } = new();

    public string SurfaceId => "vim";

    public bool TryParseToNormalized(string? wire, out NormalizedKeySequence? sequence, out string error) =>
        VimChordNotationParser.TryParseToNormalized(wire, out sequence, out error);
}
