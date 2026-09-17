#nullable enable

namespace AIGuiders.Platform.Execution.LanguageIntelligence;

/// <summary>
/// Transitional anchor span seam — SSOT migrating to RelationSpec + Kind: bracket canon.
/// </summary>
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
