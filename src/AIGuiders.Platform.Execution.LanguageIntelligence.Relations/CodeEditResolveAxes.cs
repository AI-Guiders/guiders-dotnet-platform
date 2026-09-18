#nullable enable

namespace AIGuiders.Platform.Execution.LanguageIntelligence.Relations;

/// <summary>RelationSpec.CodeEdit projection for language attach resolve (plan §10; replaces BracketAnchorSpan hot path).</summary>
public sealed record CodeEditResolveAxes(
    string? File,
    string? MemberKey,
    int? LineStart,
    int? LineEnd,
    string? ScopeKind = null,
    int? ScopeIndex = null,
    string? Role = null,
    string? XmlPath = null,
    string? Attr = null,
    string? TextNeedle = null,
    string? TypeKey = null)
{
    public static CodeEditResolveAxes Empty() => new(null, null, null, null);
}
