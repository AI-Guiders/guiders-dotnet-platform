using AIGuiders.Platform.Modeling.Notations.Keyboard;
#nullable enable

namespace AIGuiders.Platform.Notations.Keyboard;

/// <summary>KeyGesture notation reader (GUIDERS-ADR-0021).</summary>
public sealed class KeyGestureNotationReader : IKeyboardNotationReader
{
    public static KeyGestureNotationReader Instance { get; } = new();

    public string SurfaceId => "key-gesture";

    public bool TryParseToNormalized(string? wire, out NormalizedKeySequence? sequence, out string error) =>
        KeyGestureChordSyntax.TryParseToNormalized(wire, out sequence, out error);
}
