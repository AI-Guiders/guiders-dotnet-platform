#nullable enable

using GdlLanguage = AIGuiders.Platform.Modeling.Gdl.Language;

namespace AIGuiders.Platform.IntermediateRepresentation.Language;

/// <summary>
/// GUIDERS-FSHARP-ADR-0003 §4.3 cutover: anchor span SSOT via <see cref="ToModel"/> (Modeling.Gdl.Language).
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
    public static BracketAnchorSpan Empty() => FromModel(GdlLanguage.BracketAnchorSpanModule.empty);

    public GdlLanguage.BracketAnchorSpan ToModel() => new(
        FSharpInterop.OptString(File),
        FSharpInterop.OptString(MemberKey),
        FSharpInterop.OptInt(LineStart),
        FSharpInterop.OptInt(LineEnd),
        FSharpInterop.OptString(ScopeKind),
        FSharpInterop.OptInt(ScopeIndex),
        FSharpInterop.OptString(Role),
        FSharpInterop.OptString(XmlPath),
        FSharpInterop.OptString(Attr),
        FSharpInterop.OptString(Family),
        FSharpInterop.OptString(Command),
        FSharpInterop.OptString(Go),
        NestedAnchor is null
            ? Microsoft.FSharp.Core.FSharpOption<GdlLanguage.BracketAnchorSpan>.None
            : Microsoft.FSharp.Core.FSharpOption<GdlLanguage.BracketAnchorSpan>.Some(NestedAnchor.ToModel()),
        FSharpInterop.OptString(TextNeedle),
        FSharpInterop.OptString(TypeKey));

    public static BracketAnchorSpan FromModel(GdlLanguage.BracketAnchorSpan model) => new(
        FSharpInterop.OptString(model.File),
        FSharpInterop.OptString(model.MemberKey),
        FSharpInterop.OptInt(model.LineStart),
        FSharpInterop.OptInt(model.LineEnd),
        FSharpInterop.OptString(model.ScopeKind),
        FSharpInterop.OptInt(model.ScopeIndex),
        FSharpInterop.OptString(model.Role),
        FSharpInterop.OptString(model.XmlPath),
        FSharpInterop.OptString(model.Attr),
        FSharpInterop.OptString(model.Family),
        FSharpInterop.OptString(model.Command),
        FSharpInterop.OptString(model.Go),
        Microsoft.FSharp.Core.FSharpOption<GdlLanguage.BracketAnchorSpan>.get_IsSome(model.NestedAnchor)
            ? FromModel(model.NestedAnchor.Value)
            : null,
        FSharpInterop.OptString(model.TextNeedle),
        FSharpInterop.OptString(model.TypeKey));
}
