#nullable enable

using AIGuiders.Platform.IntermediateRepresentation.Bracket;
using AIGuiders.Platform.IntermediateRepresentation.Language;

namespace AIGuiders.Platform.LanguageIntelligence.Anchors;

/// <summary>Parse/format/classify via <see cref="BracketReader"/>.</summary>
public static class BracketAnchorWire
{
    static readonly Dictionary<string, string> AxisAlias = new(StringComparer.OrdinalIgnoreCase)
    {
        ["F"] = "File",
        ["File"] = "File",
        ["M"] = "Member",
        ["Member"] = "Member",
        ["L"] = "Line",
        ["Line"] = "Line",
        ["S"] = "Scope",
        ["Scope"] = "Scope",
        ["T"] = "Text",
        ["Text"] = "Text",
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
        // legacy flag → Family:navigation
        ["N"] = "Navigate",
        ["Navigate"] = "Navigate",
    };

    public static BracketAnchorSpan Parse(string bracketOrInner)
    {
        if (!BracketReader.Default.TryRead(
                bracketOrInner,
                BracketProfiles.CdpSquareKeyValue,
                out var wire,
                out var error)
            || wire is null)
            throw new ArgumentException(error);

        return SpanFromWire(wire);
    }

    static BracketAnchorSpan SpanFromWire(NormalizedBracketWire wire)
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
        string? command = null;
        string? go = null;
        string? textNeedle = null;
        BracketAnchorSpan? nested = null;
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
                    command = val.ToLowerInvariant();
                    break;
                case "Go":
                    go = val;
                    break;
                case "Anchor":
                    nested = axis.Nested is not null ? SpanFromWire(axis.Nested) : Parse(val);
                    break;
            }
        }

        if (legacyNavigate && string.IsNullOrWhiteSpace(family))
            family = "navigation";

        var span = new BracketAnchorSpan(
            file, member, lineStart, lineEnd, scopeKind, scopeIndex, role, xmlPath, attr,
            family, command, go, nested, textNeedle);
        _ = ClassifyFamily(span, out var familyError);
        if (familyError is not null)
            throw new ArgumentException(familyError);
        return span;
    }

    /// <summary>
    /// Explicit <c>Family:</c> wins; else infer code (M/S/L) vs xml (Element/Attribute).
    /// Navigation = Family or Command/Go/nested.
    /// </summary>
    public static BracketAxisFamily ClassifyFamily(BracketAnchorSpan span, out string? error)
    {
        error = null;
        var fam = NormalizeFamilyName(span.Family);
        if (fam is "navigation" or "nav")
            return BracketAxisFamily.Navigation;
        if (fam is "xml")
            return ValidateXml(span, out error) ? BracketAxisFamily.Xml : BracketAxisFamily.None;
        if (fam is "code" or "csharp" or "c#")
            return ValidateCode(span, out error) ? BracketAxisFamily.Csharp : BracketAxisFamily.None;

        var hasNav = !string.IsNullOrWhiteSpace(span.Command)
                     || !string.IsNullOrWhiteSpace(span.Go)
                     || span.NestedAnchor is not null;
        var hasCsharpStructural = !string.IsNullOrWhiteSpace(span.MemberKey)
            || !string.IsNullOrWhiteSpace(span.ScopeKind)
            || span.LineStart is not null
            || !string.IsNullOrWhiteSpace(span.TextNeedle);
        var hasXml = !string.IsNullOrWhiteSpace(span.XmlPath)
            || !string.IsNullOrWhiteSpace(span.Attr);

        if (hasNav && (hasCsharpStructural || hasXml))
        {
            // Nested Anchor may carry code/xml; outer nav axes alone are fine.
            if (!string.IsNullOrWhiteSpace(span.MemberKey)
                || !string.IsNullOrWhiteSpace(span.ScopeKind)
                || span.LineStart is not null
                || !string.IsNullOrWhiteSpace(span.TextNeedle)
                || !string.IsNullOrWhiteSpace(span.XmlPath)
                || !string.IsNullOrWhiteSpace(span.Attr))
            {
                error = "mixed_axes";
                return BracketAxisFamily.None;
            }
        }

        if (hasNav)
            return BracketAxisFamily.Navigation;

        if (hasCsharpStructural && hasXml)
        {
            error = "mixed_axes";
            return BracketAxisFamily.None;
        }

        if (hasXml)
            return ValidateXml(span, out error) ? BracketAxisFamily.Xml : BracketAxisFamily.None;

        if (hasCsharpStructural || !string.IsNullOrWhiteSpace(span.Role))
            return BracketAxisFamily.Csharp;

        // File-only → none (open path uses navigation or plain file label)
        return BracketAxisFamily.None;
    }

    static bool ValidateXml(BracketAnchorSpan span, out string? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(span.XmlPath) && !string.IsNullOrWhiteSpace(span.Attr))
        {
            error = "need_X_for_A";
            return false;
        }

        return true;
    }

    static bool ValidateCode(BracketAnchorSpan span, out string? error)
    {
        error = null;
        if (!string.IsNullOrWhiteSpace(span.XmlPath) || !string.IsNullOrWhiteSpace(span.Attr))
        {
            error = "mixed_axes";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Emit wire. Navigation → canonical names + Family.
    /// Code/xml → short aliases (compat) unless <paramref name="preferCanonical"/>.
    /// </summary>
    public static string Format(BracketAnchorSpan span, bool preferCanonical = false)
    {
        var family = ClassifyFamily(span, out _);
        var canon = preferCanonical || family == BracketAxisFamily.Navigation
                    || !string.IsNullOrWhiteSpace(span.Family);

        var parts = new List<string>();
        var famName = family switch
        {
            BracketAxisFamily.Navigation => "navigation",
            BracketAxisFamily.Xml => "xml",
            BracketAxisFamily.Csharp => "code",
            _ => NormalizeFamilyName(span.Family)
        };
        if (canon && !string.IsNullOrWhiteSpace(famName))
            parts.Add(Key("Family", canon) + ":" + famName);

        if (family == BracketAxisFamily.Navigation)
        {
            if (!string.IsNullOrWhiteSpace(span.Command))
                parts.Add(Key("Command", canon) + ":" + span.Command.Trim());
            if (!string.IsNullOrWhiteSpace(span.Go))
                parts.Add(Key("Go", canon) + ":" + span.Go.Trim());
            if (span.NestedAnchor is { } nested)
                parts.Add("Anchor:" + Format(nested, preferCanonical: true));
            return "[" + string.Join(';', parts) + "]";
        }

        if (!string.IsNullOrWhiteSpace(span.File))
            parts.Add(Key("File", canon) + ":" + span.File.Trim());
        if (!string.IsNullOrWhiteSpace(span.MemberKey))
            parts.Add(Key("Member", canon) + ":" + span.MemberKey.Trim());
        if (span.LineStart is int ls)
        {
            var lineKey = Key("Line", canon);
            parts.Add(span.LineEnd is int le && le != ls
                ? $"{lineKey}:{ls}-{le}"
                : $"{lineKey}:{ls}");
        }

        if (!string.IsNullOrWhiteSpace(span.TextNeedle))
            parts.Add(Key("Text", canon) + ":" + SanitizeTextNeedle(span.TextNeedle));

        if (!string.IsNullOrWhiteSpace(span.ScopeKind))
        {
            var kind = span.ScopeKind.Trim().ToLowerInvariant();
            var idx = span.ScopeIndex is > 0 ? span.ScopeIndex.Value : 1;
            var scopeKey = Key("Scope", canon);
            parts.Add(idx == 1 ? $"{scopeKey}:{kind}" : $"{scopeKey}:{kind}:{idx}");
        }

        if (!string.IsNullOrWhiteSpace(span.XmlPath))
            parts.Add(Key("Element", canon) + ":" + span.XmlPath.Trim());
        if (!string.IsNullOrWhiteSpace(span.Attr))
            parts.Add(Key("Attribute", canon) + ":" + span.Attr.Trim());
        if (!string.IsNullOrWhiteSpace(span.Role))
            parts.Add(Key("Kind", canon) + ":" + span.Role.Trim());

        return "[" + string.Join(';', parts) + "]";
    }

    static string Key(string canonical, bool preferCanonical) => preferCanonical
        ? canonical
        : canonical switch
        {
            "Family" => "Family",
            "File" => "F",
            "Member" => "M",
            "Line" => "L",
            "Scope" => "S",
            "Text" => "T",
            "Kind" => "K",
            "Element" => "X",
            "Attribute" => "A",
            "Command" => "Command",
            "Go" => "Go",
            _ => canonical
        };

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

    static void ParseLine(string val, out int? lineStart, out int? lineEnd)
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
