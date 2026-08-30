#nullable enable

namespace AIGuiders.Platform.Notations.Keyboard;

/// <summary>Neovim key-notation reader (GUIDERS-ADR-0021).</summary>
public sealed class NeovimNotationReader : IKeyboardNotationReader
{
    public static NeovimNotationReader Instance { get; } = new();

    public string SurfaceId => NeovimKeyDialect.Instance.SurfaceId;

    public bool TryParseToNormalized(string? wire, out NormalizedKeySequence? sequence, out string error) =>
        NeovimKeyDialect.Instance.TryParseToNormalized(wire, out sequence, out error);
}
