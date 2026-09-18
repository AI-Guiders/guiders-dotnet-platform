#nullable enable

using AIGuiders.Platform.Modeling.Notations.Bracket;
using AIGuiders.Platform.Notations.Bracket;

namespace AIGuiders.Platform.Execution.LanguageIntelligence.Relations;

/// <summary>Doc reverse-scan F/M/L wire ingest only (plan §10). Nav wires → <see cref="LegacyNavWireIngest"/>.</summary>
public static class RelationWireBoundary
{
    internal static readonly Dictionary<string, string> AxisAlias = new(StringComparer.OrdinalIgnoreCase)
    {
        ["F"] = "File",
        ["File"] = "File",
        ["M"] = "Member",
        ["Member"] = "Member",
        ["J"] = "Member",
        ["JsonPath"] = "Member",
        ["L"] = "Line",
        ["Line"] = "Line",
        ["S"] = "Scope",
        ["Scope"] = "Scope",
        ["T"] = "Type",
        ["Type"] = "Type",
        ["Text"] = "Text",
        ["Needle"] = "Text",
        ["Content"] = "Text",
        ["K"] = "Kind",
        ["Kind"] = "Kind",
        ["Role"] = "Kind",
        ["X"] = "Element",
        ["Element"] = "Element",
        ["A"] = "Attribute",
        ["Attribute"] = "Attribute",
        ["Attr"] = "Attribute",
        ["Family"] = "Family",
        ["Fam"] = "Family",
        ["Command"] = "Command",
        ["C"] = "Command",
        ["Go"] = "Go",
        ["G"] = "Go",
        ["Anchor"] = "Anchor",
        ["N"] = "Navigate",
        ["Navigate"] = "Navigate",
    };

    public static bool TryParseDocScan(string bracketOrInner, out CodeEditResolveAxes axes, out string? error)
    {
        axes = default!;
        error = null;
        if (string.IsNullOrWhiteSpace(bracketOrInner))
            return false;

        if (!BracketReader.Default.TryRead(
                bracketOrInner,
                BracketProfiles.CdpSquareKeyValue,
                out var wire,
                out error)
            || wire is null)
            return false;

        try
        {
            var parsed = ParseDocScan(wire, out var probe);
            if (WireFamilyClassifier.Classify(probe, out error) == BracketAxisFamily.Navigation)
            {
                error ??= "nav_wire";
                return false;
            }

            axes = parsed;
            return !string.IsNullOrWhiteSpace(axes.File)
                   || !string.IsNullOrWhiteSpace(axes.MemberKey)
                   || axes.LineStart is not null
                   || !string.IsNullOrWhiteSpace(axes.XmlPath);
        }
        catch (ArgumentException ex)
        {
            error = ex.Message;
            return false;
        }
    }

    internal static NormalizedBracketWire ReadWire(string bracketOrInner)
    {
        if (!BracketReader.Default.TryRead(
                bracketOrInner,
                BracketProfiles.CdpSquareKeyValue,
                out var wire,
                out var error)
            || wire is null)
            throw new ArgumentException(error);

        return wire;
    }

    static CodeEditResolveAxes ParseDocScan(NormalizedBracketWire wire, out WireFamilyClassifier.Probe probe)
    {
        string? file = null;
        string? member = null;
        int? lineStart = null;
        int? lineEnd = null;
        string? scopeKind = null;
        int? scopeIndex = null;
        string? role = null;
        string? xmlPath = null;
        string? attr = null;
        string? family = null;
        string? textNeedle = null;
        string? typeKey = null;
        var legacyNavigate = false;

        foreach (var axis in wire.Axes)
        {
            if (!AxisAlias.TryGetValue(axis.Key, out var canon))
                throw new ArgumentException($"unknown_axis:{axis.Key}");

            var val = axis.Value.Trim();
            switch (canon)
            {
                case "Family":
                    family = NormalizeFamilyName(val);
                    break;
                case "Navigate":
                    if (IsTruthy(val))
                        legacyNavigate = true;
                    break;
                case "File":
                    file = val;
                    break;
                case "Member":
                    member = val;
                    break;
                case "Line":
                    ParseLine(val, out lineStart, out lineEnd);
                    break;
                case "Type":
                    typeKey = val;
                    break;
                case "Text":
                    textNeedle = SanitizeTextNeedle(val);
                    break;
                case "Scope":
                    ParseScope(val, out scopeKind, out scopeIndex);
                    break;
                case "Kind":
                    role = val;
                    break;
                case "Element":
                    xmlPath = val;
                    break;
                case "Attribute":
                    attr = val;
                    break;
                case "Command":
                case "Go":
                case "Anchor":
                    throw new ArgumentException("nav_wire");
            }
        }

        if (legacyNavigate)
            throw new ArgumentException("nav_wire");

        probe = new WireFamilyClassifier.Probe(
            file,
            member,
            lineStart,
            scopeKind,
            role,
            xmlPath,
            attr,
            family,
            Command: null,
            Go: null,
            NestedAnchor: null,
            textNeedle,
            typeKey);

        return new CodeEditResolveAxes(
            file,
            member,
            lineStart,
            lineEnd,
            scopeKind,
            scopeIndex,
            role,
            xmlPath,
            attr,
            textNeedle,
            typeKey);
    }

    /// <summary>Strip axis separators from content needle so wire stays parseable.</summary>
    public static string SanitizeTextNeedle(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return "";
        var s = raw.Trim().Replace("\r", "").Replace("\n", " ").Replace(";", " ");
        while (s.Contains("  ", StringComparison.Ordinal))
            s = s.Replace("  ", " ", StringComparison.Ordinal);
        if (s.Length > 96)
            s = s[..96];
        return s.Trim();
    }

    internal static void ParseLine(string val, out int? lineStart, out int? lineEnd)
    {
        lineStart = null;
        lineEnd = null;
        var dash = val.IndexOf('-');
        if (dash < 0)
        {
            if (int.TryParse(val.Trim(), out var one))
            {
                lineStart = one;
                lineEnd = one;
            }

            return;
        }

        if (int.TryParse(val[..dash].Trim(), out var a)
            && int.TryParse(val[(dash + 1)..].Trim(), out var b))
        {
            lineStart = a;
            lineEnd = b;
        }
    }

    static void ParseScope(string val, out string? scopeKind, out int? scopeIndex)
    {
        scopeKind = null;
        scopeIndex = null;
        var colon = val.IndexOf(':');
        if (colon < 0)
        {
            scopeKind = val.Trim().ToLowerInvariant();
            scopeIndex = 1;
            return;
        }

        scopeKind = val[..colon].Trim().ToLowerInvariant();
        scopeIndex = int.TryParse(val[(colon + 1)..].Trim(), out var idx) && idx > 0 ? idx : 1;
    }

    static string? NormalizeFamilyName(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;
        var v = raw.Trim().ToLowerInvariant();
        return v switch
        {
            "c#" or "csharp" or "cs" => "code",
            "nav" => "navigation",
            _ => v
        };
    }

    static bool IsTruthy(string val) =>
        val.Equals("true", StringComparison.OrdinalIgnoreCase)
        || val.Equals("1", StringComparison.OrdinalIgnoreCase)
        || val.Equals("yes", StringComparison.OrdinalIgnoreCase);
}
