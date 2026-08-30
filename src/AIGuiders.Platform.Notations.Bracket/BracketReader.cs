#nullable enable
using AIGuiders.Platform.Notations;

namespace AIGuiders.Platform.Notations.Bracket;

/// <summary>Reference bracket wire parser (GUIDERS-ADR-0026 Phase 1).</summary>
public sealed class BracketReader : IBracketNotationReader
{
    public static BracketReader Default { get; } = new();

    public bool TryRead(
        string wire,
        BracketNotationProfile profile,
        out NormalizedBracketWire? normalized,
        out string error) =>
        TryRead(wire, profile, BracketAxisValuePlans.CdpCode, out normalized, out error);

    public bool TryRead(
        string wire,
        BracketNotationProfile profile,
        BracketAxisValuePlan? valuePlan,
        out NormalizedBracketWire? normalized,
        out string error)
    {
        normalized = null;
        error = "";
        if (string.IsNullOrWhiteSpace(wire))
        {
            error = "Empty wire.";
            return false;
        }

        var raw = wire.Trim();
        if (profile.AxisShape == BracketAxisShape.Opaque)
            return TryReadOpaque(raw, profile, out normalized, out error);

        var inner = raw;
        if (profile.StripOuterTerminals)
        {
            if (!TryStripTerminals(raw, profile, out inner))
                inner = raw;
        }

        if (!TryParseInner(inner, profile, valuePlan, raw, out normalized, out error))
            return false;

        return true;
    }

    static bool TryReadOpaque(
        string raw,
        BracketNotationProfile profile,
        out NormalizedBracketWire? normalized,
        out string error)
    {
        error = "";
        var inner = raw;
        if (profile.StripOuterTerminals && TryStripTerminals(raw, profile, out var stripped))
            inner = stripped;

        normalized = new NormalizedBracketWire(
            profile.Id,
            [new BracketAxis("_", profile.KvSign, inner, BracketAxisValueClasses.Opaque)],
            raw);
        return true;
    }

    static bool TryParseInner(
        string inner,
        BracketNotationProfile profile,
        BracketAxisValuePlan? valuePlan,
        string raw,
        out NormalizedBracketWire? normalized,
        out string error)
    {
        normalized = null;
        error = "";
        var segments = profile.RespectBracketDepthOnListSplit
            ? NotationListSplit.SplitTopLevel(inner, profile.ListSeparator)
            : inner.Split(profile.ListSeparator).ToList();

        var nestedKeys = profile.NestedAxisKeys is null
            ? null
            : new HashSet<string>(profile.NestedAxisKeys, StringComparer.OrdinalIgnoreCase);

        var axes = new List<BracketAxis>();
        foreach (var segment in segments)
        {
            var text = segment.Trim();
            if (text.Length == 0)
                continue;

            if (!NotationKvPair.TrySplitFirst(text, profile.KvSign, out var kv, out error))
                return false;

            NormalizedBracketWire? nested = null;
            var valueClass = ResolveValueClass(kv.Key, valuePlan);
            if (nestedKeys is not null && nestedKeys.Contains(kv.Key))
            {
                valueClass = BracketAxisValueClasses.NestedBracket;
                if (!TryParseNested(kv.Value, profile, valuePlan, out nested, out error))
                    return false;
            }

            axes.Add(new BracketAxis(kv.Key, kv.Sign, kv.Value, valueClass, nested));
        }

        if (axes.Count == 0)
        {
            error = "No axes parsed.";
            return false;
        }

        normalized = new NormalizedBracketWire(profile.Id, axes, raw);
        return true;
    }

    static bool TryParseNested(
        string value,
        BracketNotationProfile profile,
        BracketAxisValuePlan? valuePlan,
        out NormalizedBracketWire? nested,
        out string error)
    {
        nested = null;
        var inner = value.Trim();
        if (inner.StartsWith('[') && inner.EndsWith(']'))
            inner = inner[1..^1].Trim();

        return TryParseInner(inner, profile, valuePlan, value.Trim(), out nested, out error);
    }

    static string ResolveValueClass(string key, BracketAxisValuePlan? valuePlan)
    {
        if (valuePlan is not null && valuePlan.ByAxisKey.TryGetValue(key, out var wireClass))
            return wireClass;
        return BracketAxisValueClasses.Opaque;
    }

    static bool TryStripTerminals(string wire, BracketNotationProfile profile, out string inner)
    {
        inner = wire;
        if (wire.Length < profile.StartTerminal.Length + profile.EndTerminal.Length)
            return false;
        if (!wire.StartsWith(profile.StartTerminal, StringComparison.Ordinal)
            || !wire.EndsWith(profile.EndTerminal, StringComparison.Ordinal))
            return false;

        inner = wire[profile.StartTerminal.Length..^profile.EndTerminal.Length].Trim();
        return true;
    }
}
