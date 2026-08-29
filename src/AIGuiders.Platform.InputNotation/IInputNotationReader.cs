#nullable enable

namespace AIGuiders.Platform.InputNotation;

/// <summary>Parses a wire-format keyboard notation string into <see cref="NormalizedKeySequence"/> (ADR-0016).</summary>
public interface IInputNotationReader
{
    string SurfaceId { get; }

    bool TryParseToNormalized(string? wire, out NormalizedKeySequence? sequence, out string error);
}
