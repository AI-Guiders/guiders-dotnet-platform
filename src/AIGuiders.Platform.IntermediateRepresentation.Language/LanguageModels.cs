#nullable enable

namespace AIGuiders.Platform.IntermediateRepresentation.Language;

/// <summary>How confidently an <see cref="Locus"/> is bound (GUIDERS-ADR-0025).</summary>
public enum ResolveTier
{
    Text = 0,
    Syntax = 1,
    Semantic = 2,
}

/// <summary>Resolved location in a document buffer (range + optional symbol id).</summary>
public sealed record Locus(
    int Start,
    int End,
    ResolveTier Tier = ResolveTier.Text,
    string? SymbolId = null,
    string? FilePath = null);

/// <summary>Resolve input for anchor (raw wire). Prefer NormalizedBracketWire from IR.Bracket (ADR-0026).</summary>
public sealed record AnchorWire(string Value);

/// <summary>LSP-shaped single edit (language-neutral).</summary>
public sealed record TextEdit(int Start, int End, string NewText);

/// <summary>Buffer command result payload (replaces <c>EditorBufferOutcome</c> in Phase 1).</summary>
public sealed record BufferEditOutcome
{
    public string? Text { get; init; }
    public int? SelectionStart { get; init; }
    public int? SelectionEnd { get; init; }
    public string? TextMode { get; init; }
    public IReadOnlyList<TextEdit>? Edits { get; init; }

    public static BufferEditOutcome FromText(string text, int selectionStart, int selectionEnd) =>
        new() { Text = text, SelectionStart = selectionStart, SelectionEnd = selectionEnd };
}

/// <summary>EditSniper-style scope (CDP: from/till/wire/pad).</summary>
public sealed record SniperScope(
    int? FromLine = null,
    int? TillLine = null,
    string? Wire = null,
    string? Pad = null);
