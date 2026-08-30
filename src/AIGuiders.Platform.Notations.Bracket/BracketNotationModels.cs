#nullable enable

namespace AIGuiders.Platform.Notations.Bracket;

/// <summary>Paired delimiter family for bracket wires (GUIDERS-ADR-0026).</summary>
public enum BracketPairKind
{
    Angle = 0,
    Square = 1,
}

/// <summary>Key/value slot inside a bracket inner (e.g. CSX <c>F:file;M:method</c>).</summary>
public sealed record BracketSlot(string Key, string Value);

/// <summary>Neutral bracket wire after lexing — meaning resolve stays in LanguageIntelligence.</summary>
public sealed record NormalizedBracketWire(
    BracketPairKind Pair,
    string Inner,
    IReadOnlyList<BracketSlot>? Slots = null,
    string? Raw = null);

public interface IBracketNotationReader
{
    bool TryRead(string wire, out NormalizedBracketWire? normalized, out string error);
}
