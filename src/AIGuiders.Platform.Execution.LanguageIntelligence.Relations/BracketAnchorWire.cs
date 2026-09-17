#nullable enable

namespace AIGuiders.Platform.Execution.LanguageIntelligence.Relations;

/// <summary>Legacy name — use <see cref="LegacyBracketRelationWire"/>.</summary>
[Obsolete("Use LegacyBracketRelationWire. Renamed in federation Phase 1.")]
public static class BracketAnchorWire
{
    public static BracketAnchorSpan Parse(string bracketOrInner) =>
        LegacyBracketRelationWire.Parse(bracketOrInner);

    public static BracketAxisFamily ClassifyFamily(BracketAnchorSpan span, out string? error) =>
        LegacyBracketRelationWire.ClassifyFamily(span, out error);

    public static string Format(BracketAnchorSpan span, bool preferCanonical = false) =>
        LegacyBracketRelationWire.Format(span, preferCanonical);

    public static string SanitizeTextNeedle(string? raw) =>
        LegacyBracketRelationWire.SanitizeTextNeedle(raw);
}
