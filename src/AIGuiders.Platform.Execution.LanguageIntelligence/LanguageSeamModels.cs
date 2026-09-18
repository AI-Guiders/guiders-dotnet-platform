#nullable enable

using System.Collections.Generic;
using System.Linq;
using ModelingLanguage = AIGuiders.Platform.Modeling.Language;
using Microsoft.FSharp.Core;

namespace AIGuiders.Platform.Execution.LanguageIntelligence;

/// <summary>Legacy char-offset locate seam. Canonical locate is Relations.Locus DU (plan §2.4).</summary>
public sealed record Locus(
    int Start,
    int End,
    ResolveTier Tier = ResolveTier.Syntax,
    string? SymbolId = null,
    string? FilePath = null)
{
    public static Locus OfRange(int start, int end) => new(start, end, ResolveTier.Syntax);
}

/// <summary>Resolve input for relation (raw wire). Prefer Kind: bracket wire / RelationSpec (ADR-0026).</summary>
public sealed record RelationWire(string Value);

/// <summary>Legacy alias — prefer <see cref="RelationWire"/>.</summary>
public sealed record AnchorWire(string Value)
{
    public RelationWire ToRelationWire() => new(Value);
}

/// <summary>LSP-shaped single edit (language-neutral).</summary>
public sealed record TextEdit(int Start, int End, string NewText)
{
    public ModelingLanguage.TextEdit ToModel() => new(Start, End, NewText);

    public static TextEdit FromModel(ModelingLanguage.TextEdit model) => new(model.Start, model.End, model.NewText);
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
        FromModel(ModelingLanguage.BufferEditOutcomeModule.fromText(text, selectionStart, selectionEnd));

    public ModelingLanguage.BufferEditOutcome ToModel() => new(
        FSharpInterop.OptString(Text),
        FSharpInterop.OptInt(SelectionStart),
        FSharpInterop.OptInt(SelectionEnd),
        FSharpInterop.OptString(TextMode),
        Edits is null
            ? FSharpOption<IReadOnlyList<ModelingLanguage.TextEdit>>.None
            : FSharpOption<IReadOnlyList<ModelingLanguage.TextEdit>>.Some(
                Edits.Select(static edit => edit.ToModel()).ToList()));

    public static BufferEditOutcome FromModel(ModelingLanguage.BufferEditOutcome model) => new()
    {
        Text = FSharpInterop.OptString(model.Text),
        SelectionStart = FSharpInterop.OptInt(model.SelectionStart),
        SelectionEnd = FSharpInterop.OptInt(model.SelectionEnd),
        TextMode = FSharpInterop.OptString(model.TextMode),
        Edits = model.Edits is not null && FSharpOption<IReadOnlyList<ModelingLanguage.TextEdit>>.get_IsSome(model.Edits)
            ? model.Edits.Value.Select(static edit => TextEdit.FromModel(edit)).ToList()
            : null,
    };
}

/// <summary>EditSniper-style scope (CDP: from/till/wire/pad) — Execution-only until RelationSpec canon.</summary>
public sealed record SniperScope(
    int? FromLine = null,
    int? TillLine = null,
    string? Wire = null,
    string? Pad = null)
{
    public static SniperScope Empty() => new();
}
