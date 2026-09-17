#nullable enable

using AIGuiders.Platform.IntermediateRepresentation.Language;

namespace AIGuiders.Platform.Execution.LanguageIntelligence.Relations;

/// <summary>Legacy name — use <see cref="BracketRelationWire"/>.</summary>
[Obsolete("Use BracketRelationWire. Renamed in federation Phase 1.")]
public static class BracketAnchorWire
{
    public static BracketAnchorSpan Parse(string bracketOrInner) =>
        BracketRelationWire.Parse(bracketOrInner);

    public static BracketAxisFamily ClassifyFamily(BracketAnchorSpan span, out string? error) =>
        BracketRelationWire.ClassifyFamily(span, out error);

    public static string Format(BracketAnchorSpan span, bool preferCanonical = false) =>
        BracketRelationWire.Format(span, preferCanonical);

    public static string SanitizeTextNeedle(string? raw) =>
        BracketRelationWire.SanitizeTextNeedle(raw);
}
