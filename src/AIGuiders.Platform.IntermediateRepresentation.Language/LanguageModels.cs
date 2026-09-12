#nullable enable

using System.Collections.Generic;
using System.Linq;
using GdlLanguage = AIGuiders.Platform.Modeling.Gdl.Language;
using Microsoft.FSharp.Core;

namespace AIGuiders.Platform.IntermediateRepresentation.Language;

/// <summary>Resolved location in a document buffer (range + optional symbol id).</summary>
public sealed record Locus(
    int Start,
    int End,
    ResolveTier Tier = ResolveTier.Text,
    string? SymbolId = null,
    string? FilePath = null)
{
    public static Locus OfRange(int start, int end) =>
        FromModel(GdlLanguage.LocusModule.ofRange(start, end));

    public GdlLanguage.Locus ToModel() => new(
        Start,
        End,
        Tier,
        FSharpInterop.OptString(SymbolId),
        FSharpInterop.OptString(FilePath));

    public static Locus FromModel(GdlLanguage.Locus model) => new(
        model.Start,
        model.End,
        model.Tier,
        FSharpInterop.OptString(model.SymbolId),
        FSharpInterop.OptString(model.FilePath));
}

/// <summary>Resolve input for anchor (raw wire). Prefer NormalizedBracketWire from IR.Bracket (ADR-0026).</summary>
public sealed record AnchorWire(string Value)
{
    public GdlLanguage.AnchorWire ToModel() => new(Value);

    public static AnchorWire FromModel(GdlLanguage.AnchorWire model) => new(model.Value);
}

/// <summary>LSP-shaped single edit (language-neutral).</summary>
public sealed record TextEdit(int Start, int End, string NewText)
{
    public GdlLanguage.TextEdit ToModel() => new(Start, End, NewText);

    public static TextEdit FromModel(GdlLanguage.TextEdit model) => new(model.Start, model.End, model.NewText);
}

/// <summary>Buffer command result payload (replaces <c>EditorBufferOutcome</c> in Phase 1).</summary>
public sealed record BufferEditOutcome
{
    public string? Text { get; init; }
    public int? SelectionStart { get; init; }
    public int? SelectionEnd { get; init; }
    public string? TextMode { get; init; }
    public IReadOnlyList<TextEdit>? Edits { get; init; }

    public static BufferEditOutcome FromText(string text, int selectionStart, int selectionEnd) =>
        FromModel(GdlLanguage.BufferEditOutcomeModule.fromText(text, selectionStart, selectionEnd));

    public GdlLanguage.BufferEditOutcome ToModel() => new(
        FSharpInterop.OptString(Text),
        FSharpInterop.OptInt(SelectionStart),
        FSharpInterop.OptInt(SelectionEnd),
        FSharpInterop.OptString(TextMode),
        Edits is null
            ? FSharpOption<IReadOnlyList<GdlLanguage.TextEdit>>.None
            : FSharpOption<IReadOnlyList<GdlLanguage.TextEdit>>.Some(
                Edits.Select(static edit => edit.ToModel()).ToList()));

    public static BufferEditOutcome FromModel(GdlLanguage.BufferEditOutcome model) => new()
    {
        Text = FSharpInterop.OptString(model.Text),
        SelectionStart = FSharpInterop.OptInt(model.SelectionStart),
        SelectionEnd = FSharpInterop.OptInt(model.SelectionEnd),
        TextMode = FSharpInterop.OptString(model.TextMode),
        Edits = FSharpOption<IReadOnlyList<GdlLanguage.TextEdit>>.get_IsSome(model.Edits)
            ? model.Edits.Value.Select(static edit => TextEdit.FromModel(edit)).ToList()
            : null,
    };
}

/// <summary>EditSniper-style scope (CDP: from/till/wire/pad).</summary>
public sealed record SniperScope(
    int? FromLine = null,
    int? TillLine = null,
    string? Wire = null,
    string? Pad = null)
{
    public static SniperScope Empty() => FromModel(GdlLanguage.SniperScopeModule.empty);

    public GdlLanguage.SniperScope ToModel() => new(
        FSharpInterop.OptInt(FromLine),
        FSharpInterop.OptInt(TillLine),
        FSharpInterop.OptString(Wire),
        FSharpInterop.OptString(Pad));

    public static SniperScope FromModel(GdlLanguage.SniperScope model) => new(
        FSharpInterop.OptInt(model.FromLine),
        FSharpInterop.OptInt(model.TillLine),
        FSharpInterop.OptString(model.Wire),
        FSharpInterop.OptString(model.Pad));
}
