#nullable enable

namespace AIGuiders.Platform.Execution.LanguageIntelligence;

/// <summary>
/// Transitional anchor span seam — legacy F/M/L wire IR only; resolve hot path uses RelationSpec (plan §10 delete).
/// </summary>
[Obsolete("Legacy F/M/L wire IR. Use RelationSpec + CodeEdit resolve path. Plan §10 delete BracketAnchorSpan.")]
public sealed record BracketAnchorSpan(
    string? File,
    string? MemberKey,
    int? LineStart,
    int? LineEnd,
    string? ScopeKind = null,
    int? ScopeIndex = null,
    string? Role = null,
    string? XmlPath = null,
    string? Attr = null,
    string? Family = null,
    string? Command = null,
    string? Go = null,
    BracketAnchorSpan? NestedAnchor = null,
    string? TextNeedle = null,
    string? TypeKey = null)
{
    public static BracketAnchorSpan Empty() => new(null, null, null, null);
}
