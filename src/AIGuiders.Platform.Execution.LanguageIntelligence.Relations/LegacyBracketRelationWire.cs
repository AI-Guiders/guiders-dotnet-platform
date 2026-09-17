#nullable enable

using AIGuiders.Platform.Modeling.Notations.Bracket;

namespace AIGuiders.Platform.Execution.LanguageIntelligence.Relations;

/// <summary>Obsolete alias for <see cref="RelationWireBoundary"/>; retained for transitional callers.</summary>
[Obsolete("Use RelationWireBoundary at Execution parse boundary.")]
public static class LegacyBracketRelationWire
{
    public static BracketAnchorSpan Parse(string bracketOrInner) =>
        RelationWireBoundary.Parse(bracketOrInner);

    public static BracketAxisFamily ClassifyFamily(BracketAnchorSpan span, out string? error) =>
        RelationWireBoundary.ClassifyFamily(span, out error);

    public static string Format(BracketAnchorSpan span, bool preferCanonical = false) =>
        RelationWireBoundary.Format(span, preferCanonical);

    public static string SanitizeTextNeedle(string? raw) =>
        RelationWireBoundary.SanitizeTextNeedle(raw);
}
