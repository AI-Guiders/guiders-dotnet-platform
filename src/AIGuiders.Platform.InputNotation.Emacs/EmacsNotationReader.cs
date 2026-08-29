#nullable enable

namespace AIGuiders.Platform.InputNotation;

/// <summary>Emacs kbd notation reader (GUIDERS-ADR-0016).</summary>
public sealed class EmacsNotationReader : IInputNotationReader
{
    public static EmacsNotationReader Instance { get; } = new();

    public string SurfaceId => EmacsKbdDialect.Instance.SurfaceId;

    public bool TryParseToNormalized(string? wire, out NormalizedKeySequence? sequence, out string error) =>
        EmacsKbdDialect.Instance.TryParseToNormalized(wire, out sequence, out error);
}
