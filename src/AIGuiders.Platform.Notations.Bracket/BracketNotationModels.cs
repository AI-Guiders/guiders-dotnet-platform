#nullable enable

namespace AIGuiders.Platform.Notations.Bracket;

/// <summary>
/// Universal KV wire atom: <c>Key</c> + <c>Sign</c> + <c>Value</c> (GUIDERS-ADR-0026).
/// Bracket axes, console <c>doc=README</c>, inner <c>for:2</c> — same shape, different signs.
/// </summary>
public sealed record NotationKvPair(string Key, char Sign, string Value)
{
    /// <summary>Split on first <paramref name="sign"/> only (value may contain more signs).</summary>
    public static bool TrySplitFirst(string segment, char sign, out NotationKvPair pair, out string error)
    {
        pair = null!;
        error = "";
        if (string.IsNullOrWhiteSpace(segment))
        {
            error = "Empty segment.";
            return false;
        }

        segment = segment.Trim();
        var index = segment.IndexOf(sign);
        if (index <= 0)
        {
            error = $"Missing KV sign '{sign}'.";
            return false;
        }

        pair = new NotationKvPair(segment[..index].Trim(), sign, segment[(index + 1)..].Trim());
        return true;
    }
}

/// <summary>How inner text tokenizes into axes (GUIDERS-ADR-0026).</summary>
public enum BracketAxisShape
{
    /// <summary>Each axis is <see cref="NotationKvPair"/> with profile <see cref="BracketNotationProfile.KvSign"/>.</summary>
    KeyValue = 0,

    /// <summary>Whole inner is one blob; planet maps downstream.</summary>
    Opaque = 1,
}

/// <summary>
/// Parameterized bracket wire contract. Federation SSOT shape; planets supply profile instances.
/// </summary>
public sealed record BracketNotationProfile(
    string Id,
    string StartTerminal,
    string EndTerminal,
    char ListSeparator = ';',
    char KvSign = ':',
    BracketAxisShape AxisShape = BracketAxisShape.KeyValue,
    bool StripOuterTerminals = true,
    bool RespectBracketDepthOnListSplit = true,
    IReadOnlyList<string>? NestedAxisKeys = null)
{
    /// <summary>Legacy name for <see cref="KvSign"/>.</summary>
    [Obsolete("Use KvSign — KV = Key + Sign + Value.")]
    public char PairDelimiter => KvSign;

    /// <summary>Legacy name for <see cref="ListSeparator"/>.</summary>
    [Obsolete("Use ListSeparator.")]
    public char AxisSeparator => ListSeparator;

    /// <summary>Legacy name.</summary>
    [Obsolete("Use RespectBracketDepthOnListSplit.")]
    public bool RespectBracketDepthOnAxisSplit => RespectBracketDepthOnListSplit;
}

/// <summary>One axis inside a bracket pair — envelope-level KV.</summary>
public sealed record BracketAxis(
    string Key,
    char Sign,
    string Value,
    string ValueWireClass = BracketAxisValueClasses.Opaque,
    NormalizedBracketWire? Nested = null)
{
    public NotationKvPair ToKvPair() => new(Key, Sign, Value);
}

/// <summary>Second-pass shape for axis <see cref="BracketAxis.Value"/> (after envelope KV split).</summary>
public static class BracketAxisValueClasses
{
    public const string Opaque = "opaque";
    public const string CommandPath = "command.path";
    /// <summary>Re-parse value as <see cref="NotationKvPair"/> with planet/chosen sign (e.g. <c>for:2</c>).</summary>
    public const string Kv = "notation.kv";
    public const string LineRange = "line.range";
    public const string NestedBracket = "bracket.nested";

    [Obsolete("Use Kv — colon is just KvSign ':'")]
    public const string ArgumentColon = Kv;
}

/// <summary>Planet table: axis key → value wire class; optional per-axis KV sign for pass 2.</summary>
public sealed record BracketAxisValuePlan(
    IReadOnlyDictionary<string, string> ByAxisKey,
    char DefaultValueKvSign = ':');

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

public sealed record BracketAxisAliasMap(IReadOnlyDictionary<string, string> Aliases);

public static class BracketProfiles
{
    public static BracketNotationProfile CdpSquareKeyValue { get; } = new(
        "bracket.cdp-square-kv",
        "[",
        "]",
        NestedAxisKeys: ["Anchor"]);

    public static BracketNotationProfile SquareKeyValue => CdpSquareKeyValue;

    public static BracketNotationProfile AngleOpaque { get; } = new(
        "bracket.angle-opaque",
        "<",
        ">",
        AxisShape: BracketAxisShape.Opaque);
}

public static class BracketAxisValuePlans
{
    public static BracketAxisValuePlan CdpCode { get; } = new(
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["F"] = BracketAxisValueClasses.CommandPath,
            ["File"] = BracketAxisValueClasses.CommandPath,
            ["M"] = BracketAxisValueClasses.Opaque,
            ["Member"] = BracketAxisValueClasses.Opaque,
            ["L"] = BracketAxisValueClasses.LineRange,
            ["Line"] = BracketAxisValueClasses.LineRange,
            ["S"] = BracketAxisValueClasses.Kv,
            ["Scope"] = BracketAxisValueClasses.Kv,
            ["K"] = BracketAxisValueClasses.Kv,
            ["Kind"] = BracketAxisValueClasses.Kv,
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
        },
        DefaultValueKvSign: ':');

    public static BracketAxisValuePlan ForgeFrgCompound { get; } = new(
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["FRG"] = BracketAxisValueClasses.CommandPath,
        });
}
