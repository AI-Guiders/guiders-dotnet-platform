#nullable enable

namespace AIGuiders.Platform.InputNotation;

/// <summary>KeyGesture notation reader (GUIDERS-ADR-0016).</summary>
public sealed class KeyGestureNotationReader : IInputNotationReader
{
    public static KeyGestureNotationReader Instance { get; } = new();

    public string SurfaceId => "key-gesture";

    public bool TryParseToNormalized(string? wire, out NormalizedKeySequence? sequence, out string error) =>
        KeyGestureChordSyntax.TryParseToNormalized(wire, out sequence, out error);
}
