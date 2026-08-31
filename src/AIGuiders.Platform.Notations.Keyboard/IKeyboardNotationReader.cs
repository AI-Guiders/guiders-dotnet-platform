using AIGuiders.Platform.IntermediateRepresentation.Keyboard;
#nullable enable

namespace AIGuiders.Platform.Notations.Keyboard;

/// <summary>Parses a wire-format keyboard notation string into <see cref="NormalizedKeySequence"/> (ADR-0016).</summary>
public interface IKeyboardNotationReader
{
    string SurfaceId { get; }

    bool TryParseToNormalized(string? wire, out NormalizedKeySequence? sequence, out string error);
}
