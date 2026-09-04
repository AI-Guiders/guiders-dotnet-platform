#nullable enable

namespace AIGuiders.Platform.IntermediateRepresentation.Language;

/// <summary>CDP/CIDE anchor span — families code (csharp, fsharp, json) / xml / navigation (CIDE 0128 / 0186).</summary>
public enum BracketAxisFamily
{
    None,
    Csharp,
    Xml,
    Navigation,
    Fsharp,
    Json,
}

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
    string? TextNeedle = null);
