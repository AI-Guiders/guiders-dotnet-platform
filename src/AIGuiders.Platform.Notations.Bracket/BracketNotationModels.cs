#nullable enable

namespace AIGuiders.Platform.Notations.Bracket;

/// <summary>How inner text tokenizes into <see cref="BracketAxis"/> records (GUIDERS-ADR-0026).</summary>
public enum BracketAxisShape
{
    /// <summary>Each axis is <c>Key</c><see cref="BracketNotationProfile.PairDelimiter"/><c>Value</c>.</summary>
    KeyValue = 0,

    /// <summary>Whole inner is one axis; planet maps <see cref="BracketAxis.Value"/> downstream.</summary>
    Opaque = 1,
}

/// <summary>
/// Parameterized bracket wire contract. Federation SSOT shape; planets supply profile instances
/// (terminals, delimiters, axis shape).
/// </summary>
public sealed record BracketNotationProfile(
    string Id,
    string StartTerminal,
    string EndTerminal,
    char AxisSeparator = ';',
    char PairDelimiter = ':',
    BracketAxisShape AxisShape = BracketAxisShape.KeyValue);

/// <summary>One axis inside a bracket pair. KV axes use <see cref="Key"/> + <see cref="Value"/>.</summary>
public sealed record BracketAxis(string Key, string Value);

/// <summary>Neutral bracket wire after lexing — axis meaning stays in planet / LanguageIntelligence.</summary>
public sealed record NormalizedBracketWire(
    string ProfileId,
    IReadOnlyList<BracketAxis> Axes,
    string Raw);

public interface IBracketNotationReader
{
    bool TryRead(
        string wire,
        BracketNotationProfile profile,
        out NormalizedBracketWire? normalized,
        out string error);
}

/// <summary>Federation well-known profiles (conformance fixtures; planets may extend).</summary>
public static class BracketProfiles
{
    /// <summary>CSX-style anchor: <c>[F:file;M:method]</c>.</summary>
    public static BracketNotationProfile SquareKeyValue { get; } = new(
        "bracket.square-kv",
        "[",
        "]");

    /// <summary>Keyboard quarry: <c>&lt;C-k&gt;</c> — opaque inner, no axis split.</summary>
    public static BracketNotationProfile AngleOpaque { get; } = new(
        "bracket.angle-opaque",
        "<",
        ">",
        AxisShape: BracketAxisShape.Opaque);
}
