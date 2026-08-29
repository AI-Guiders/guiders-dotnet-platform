#nullable enable

namespace AIGuiders.Platform.InputNotation;

/// <summary>Neovim key-notation reader (GUIDERS-ADR-0016).</summary>
public sealed class NeovimNotationReader : IInputNotationReader
{
    public static NeovimNotationReader Instance { get; } = new();

    public string SurfaceId => NeovimKeyDialect.Instance.SurfaceId;

    public bool TryParseToNormalized(string? wire, out NormalizedKeySequence? sequence, out string error) =>
        NeovimKeyDialect.Instance.TryParseToNormalized(wire, out sequence, out error);
}
