#nullable enable

namespace AIGuiders.Platform.Notations.Bracket;

/// <summary>How inner text tokenizes into <see cref="BracketAxis"/> records (GUIDERS-ADR-0026).</summary>
public enum BracketAxisShape
{
    /// <summary>Each axis is <c>Key</c><see cref="BracketNotationProfile.PairDelimiter"/><c>Value</c> (first delimiter only).</summary>
    KeyValue = 0,

    /// <summary>Whole inner is one axis; planet maps <see cref="BracketAxis.Value"/> downstream.</summary>
    Opaque = 1,
}

/// <summary>
/// Parameterized bracket wire contract. Federation SSOT shape; planets supply profile instances
/// (terminals, delimiters, axis shape, nested-axis keys).
/// </summary>
/// <param name="NestedAxisKeys">
/// Axis keys whose value is a nested bracket wire (CDP: <c>Anchor:[F:…;M:…]</c>).
/// </param>
/// <param name="RespectBracketDepthOnAxisSplit">
/// When true, <see cref="AxisSeparator"/> splits only at bracket depth 0 (CDP <c>BracketLocate.SplitTopLevel</c>).
/// </param>
public sealed record BracketNotationProfile(
    string Id,
    string StartTerminal,
    string EndTerminal,
    char AxisSeparator = ';',
    char PairDelimiter = ':',
    BracketAxisShape AxisShape = BracketAxisShape.KeyValue,
    bool StripOuterTerminals = true,
    bool RespectBracketDepthOnAxisSplit = true,
    IReadOnlyList<string>? NestedAxisKeys = null);

/// <summary>One axis inside a bracket pair. KV axes use <see cref="Key"/> + <see cref="Value"/>.</summary>
/// <param name="Nested">
/// Populated when <see cref="Key"/> is listed in profile <c>NestedAxisKeys</c> and value parses as nested wire.
/// </param>
public sealed record BracketAxis(
    string Key,
    string Value,
    NormalizedBracketWire? Nested = null);

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

/// <summary>Optional planet extension: map short axis letters to canonical names (CDP F→File, M→Member, …).</summary>
public sealed record BracketAxisAliasMap(IReadOnlyDictionary<string, string> Aliases);

/// <summary>Federation well-known profiles (conformance fixtures; planets may extend).</summary>
public static class BracketProfiles
{
    /// <summary>
    /// CDP/CIDE CSX anchor (0128/0186): <c>[F:file;M:method;L:12-34;Anchor:[…]]</c>.
    /// Value may contain extra <see cref="BracketNotationProfile.PairDelimiter"/> chars (<c>K:Parameter:x</c>, <c>S:if:2</c>).
    /// </summary>
    public static BracketNotationProfile CdpSquareKeyValue { get; } = new(
        "bracket.cdp-square-kv",
        "[",
        "]",
        NestedAxisKeys: ["Anchor"]);

    /// <summary>Legacy alias.</summary>
    public static BracketNotationProfile SquareKeyValue => CdpSquareKeyValue;

    /// <summary>Keyboard quarry: <c>&lt;C-k&gt;</c> — opaque inner, no axis split.</summary>
    public static BracketNotationProfile AngleOpaque { get; } = new(
        "bracket.angle-opaque",
        "<",
        ">",
        AxisShape: BracketAxisShape.Opaque);
}
