#nullable enable

namespace AIGuiders.Platform.Execution.LanguageIntelligence.Relations;

/// <summary>Classify bracket wire families from resolve axes (plan §10; replaces LegacyWireSpan classify).</summary>
public static class WireFamilyClassifier
{
    public sealed record Probe(
        string? File,
        string? MemberKey,
        int? LineStart,
        string? ScopeKind,
        string? Role,
        string? XmlPath,
        string? Attr,
        string? Family,
        string? Command,
        string? Go,
        Probe? NestedAnchor,
        string? TextNeedle,
        string? TypeKey)
    {
        public static Probe FromCodeEdit(CodeEditResolveAxes axes) => new(
            axes.File,
            axes.MemberKey,
            axes.LineStart,
            axes.ScopeKind,
            axes.Role,
            axes.XmlPath,
            axes.Attr,
            Family: null,
            Command: null,
            Go: null,
            NestedAnchor: null,
            axes.TextNeedle,
            axes.TypeKey);

        public static Probe FromNav(NavResolveAxes axes) => new(
            axes.File,
            axes.Member,
            axes.Line,
            ScopeKind: null,
            Role: null,
            XmlPath: null,
            Attr: null,
            Family: "navigation",
            axes.Command,
            axes.Go,
            NestedAnchor: null,
            TextNeedle: null,
            TypeKey: null);
    }

    public static BracketAxisFamily Classify(Probe span, out string? error)
    {
        error = null;
        var fam = NormalizeFamilyName(span.Family);
        if (fam is "navigation" or "nav")
            return BracketAxisFamily.Navigation;
        if (fam is "xml")
            return ValidateXml(span, out error) ? BracketAxisFamily.Xml : BracketAxisFamily.None;
        if (fam is "code" or "csharp" or "c#")
            return ValidateCode(span, out error) ? BracketAxisFamily.Csharp : BracketAxisFamily.None;
        if (fam is "fsharp" or "fs" or "f#")
            return ValidateCode(span, out error) ? BracketAxisFamily.Fsharp : BracketAxisFamily.None;
        if (fam is "json" or "j")
            return ValidateJson(span, out error) ? BracketAxisFamily.Json : BracketAxisFamily.None;

        var hasNav = !string.IsNullOrWhiteSpace(span.Command)
                     || !string.IsNullOrWhiteSpace(span.Go)
                     || span.NestedAnchor is not null;
        var memberKey = span.MemberKey;
        var hasJson = memberKey is { Length: > 0 } mk && mk.TrimStart().StartsWith("$", StringComparison.Ordinal);
        var hasCsharpStructural = (!string.IsNullOrWhiteSpace(memberKey) && !hasJson)
            || !string.IsNullOrWhiteSpace(span.ScopeKind)
            || span.LineStart is not null
            || !string.IsNullOrWhiteSpace(span.TypeKey)
            || !string.IsNullOrWhiteSpace(span.TextNeedle);
        var hasXml = !string.IsNullOrWhiteSpace(span.XmlPath)
            || !string.IsNullOrWhiteSpace(span.Attr);

        if (hasNav && (hasCsharpStructural || hasXml || hasJson))
        {
            if (!string.IsNullOrWhiteSpace(span.MemberKey)
                || !string.IsNullOrWhiteSpace(span.ScopeKind)
                || span.LineStart is not null
                || !string.IsNullOrWhiteSpace(span.TypeKey)
                || !string.IsNullOrWhiteSpace(span.TextNeedle)
                || !string.IsNullOrWhiteSpace(span.XmlPath)
                || !string.IsNullOrWhiteSpace(span.Attr)
                || hasJson)
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

        if (hasJson && hasXml)
        {
            error = "mixed_axes";
            return BracketAxisFamily.None;
        }

        if (hasJson)
            return ValidateJson(span, out error) ? BracketAxisFamily.Json : BracketAxisFamily.None;

        if (hasXml)
            return ValidateXml(span, out error) ? BracketAxisFamily.Xml : BracketAxisFamily.None;

        if (hasCsharpStructural || !string.IsNullOrWhiteSpace(span.Role))
            return BracketAxisFamily.Csharp;

        return BracketAxisFamily.None;
    }

    static bool ValidateXml(Probe span, out string? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(span.XmlPath) && !string.IsNullOrWhiteSpace(span.Attr))
        {
            error = "need_X_for_A";
            return false;
        }

        return true;
    }

    static bool ValidateJson(Probe span, out string? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(span.MemberKey))
        {
            error = "need_J_for_json";
            return false;
        }

        if (!string.IsNullOrWhiteSpace(span.XmlPath) || !string.IsNullOrWhiteSpace(span.Attr))
        {
            error = "mixed_axes";
            return false;
        }

        return true;
    }

    static bool ValidateCode(Probe span, out string? error)
    {
        error = null;
        if (!string.IsNullOrWhiteSpace(span.XmlPath) || !string.IsNullOrWhiteSpace(span.Attr))
        {
            error = "mixed_axes";
            return false;
        }

        return true;
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
}
