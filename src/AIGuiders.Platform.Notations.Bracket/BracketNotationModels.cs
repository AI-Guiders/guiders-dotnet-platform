#nullable enable

namespace AIGuiders.Platform.Notations.Bracket;

public enum BracketAxisShape
{
    KeyValue = 0,
    Opaque = 1,
}

public sealed record BracketNotationProfile(
    string Id,
    string StartTerminal,
    string EndTerminal,
    char ListSeparator = ';',
    char KvSign = ':',
    BracketAxisShape AxisShape = BracketAxisShape.KeyValue,
    bool StripOuterTerminals = true,
    bool RespectBracketDepthOnListSplit = true,
    IReadOnlyList<string>? NestedAxisKeys = null);

public sealed record BracketAxis(
    string Key,
    char Sign,
    string Value,
    string ValueWireClass = BracketAxisValueClasses.Opaque,
    NormalizedBracketWire? Nested = null)
{
    public global::AIGuiders.Platform.Notations.NotationKvPair ToKvPair() => new(Key, Sign, Value);
}

public static class BracketAxisValueClasses
{
    public const string Opaque = "opaque";
    public const string CommandPath = "command.path";
    public const string Kv = "notation.kv";
    public const string LineRange = "line.range";
    public const string NestedBracket = "bracket.nested";
}

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
        });

    public static BracketAxisValuePlan ForgeFrgCompound { get; } = new(
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["FRG"] = BracketAxisValueClasses.CommandPath,
        });
}
