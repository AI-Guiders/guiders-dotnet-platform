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
/// <param name="ValueWireClass">
/// Optional second-pass notation (compose Command/Argument micro-readers). See <see cref="BracketAxisValueClasses"/>.
/// </param>
/// <param name="Nested">
/// Populated when <see cref="ValueWireClass"/> is <see cref="BracketAxisValueClasses.NestedBracket"/>
/// or key is in profile <c>NestedAxisKeys</c>.
/// </param>
public sealed record BracketAxis(
    string Key,
    string Value,
    string ValueWireClass = BracketAxisValueClasses.Opaque,
    NormalizedBracketWire? Nested = null);

/// <summary>
/// Second-pass value notation inside an axis (same pattern as Argument <c>wire_class</c> — ADR-0021).
/// Bracket splits envelope; value class selects Command/Argument/Bracket micro-reader.
/// </summary>
public static class BracketAxisValueClasses
{
    public const string Opaque = "opaque";
    /// <summary>Slash-like segments: <c>pilot/issues/7</c>, <c>src/Foo.cs</c> (Notations.Command path).</summary>
    public const string CommandPath = "command.path";
    /// <summary>Colon slots in value: <c>for:2</c>, <c>Parameter:Run</c> (Notations.Argument colon delimited).</summary>
    public const string ArgumentColon = "argument.colon";
    /// <summary>Line span: <c>12</c>, <c>12-34</c>.</summary>
    public const string LineRange = "line.range";
    /// <summary>Recursive bracket envelope on axis value.</summary>
    public const string NestedBracket = "bracket.nested";
}

/// <summary>Planet table: axis key → <see cref="BracketAxisValueClasses"/> for value second-pass.</summary>
public sealed record BracketAxisValuePlan(IReadOnlyDictionary<string, string> ByAxisKey);

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

/// <summary>Well-known axis value plans (compose Notations Command/Argument inside bracket axes).</summary>
public static class BracketAxisValuePlans
{
    /// <summary>CDP code + navigation axes (0128/0186).</summary>
    public static BracketAxisValuePlan CdpCode { get; } = new(
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["F"] = BracketAxisValueClasses.CommandPath,
            ["File"] = BracketAxisValueClasses.CommandPath,
            ["M"] = BracketAxisValueClasses.Opaque,
            ["Member"] = BracketAxisValueClasses.Opaque,
            ["L"] = BracketAxisValueClasses.LineRange,
            ["Line"] = BracketAxisValueClasses.LineRange,
            ["S"] = BracketAxisValueClasses.ArgumentColon,
            ["Scope"] = BracketAxisValueClasses.ArgumentColon,
            ["K"] = BracketAxisValueClasses.ArgumentColon,
            ["Kind"] = BracketAxisValueClasses.ArgumentColon,
            ["T"] = BracketAxisValueClasses.Opaque,
            ["Text"] = BracketAxisValueClasses.Opaque,
            ["X"] = BracketAxisValueClasses.CommandPath,
            ["Element"] = BracketAxisValueClasses.CommandPath,
            ["A"] = BracketAxisValueClasses.Opaque,
            ["Attribute"] = BracketAxisValueClasses.Opaque,
            ["Anchor"] = BracketAxisValueClasses.NestedBracket,
            ["Command"] = BracketAxisValueClasses.Opaque,
            ["Go"] = BracketAxisValueClasses.Opaque,
            ["Family"] = BracketAxisValueClasses.Opaque,
        });

    /// <summary>Forge FRG head axis + optional CDP code tail (ADR 0159).</summary>
    public static BracketAxisValuePlan ForgeFrgCompound { get; } = new(
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["FRG"] = BracketAxisValueClasses.CommandPath,
        });
}
