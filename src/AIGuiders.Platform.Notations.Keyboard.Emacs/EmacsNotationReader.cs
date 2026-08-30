#nullable enable

namespace AIGuiders.Platform.Notations.Keyboard;

/// <summary>Emacs kbd notation reader (GUIDERS-ADR-0021).</summary>
public sealed class EmacsNotationReader : IKeyboardNotationReader
{
    public static EmacsNotationReader Instance { get; } = new();

    public string SurfaceId => EmacsKbdDialect.Instance.SurfaceId;

    public bool TryParseToNormalized(string? wire, out NormalizedKeySequence? sequence, out string error) =>
        EmacsKbdDialect.Instance.TryParseToNormalized(wire, out sequence, out error);
}
