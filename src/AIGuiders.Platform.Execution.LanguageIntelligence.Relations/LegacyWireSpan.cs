#nullable enable

namespace AIGuiders.Platform.Execution.LanguageIntelligence.Relations;

/// <summary>Legacy F/M/L wire parse result — conformance-only until Kind: canon fully retires axis wires (plan §10).</summary>
public sealed record LegacyWireSpan(
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
    LegacyWireSpan? NestedAnchor = null,
    string? TextNeedle = null,
    string? TypeKey = null)
{
    public static LegacyWireSpan Empty() => new(null, null, null, null);

    public CodeEditResolveAxes ToCodeEditAxes() =>
        new(
            File,
            MemberKey,
            LineStart,
            LineEnd,
            ScopeKind,
            ScopeIndex,
            Role,
            XmlPath,
            Attr,
            TextNeedle,
            TypeKey);
}
