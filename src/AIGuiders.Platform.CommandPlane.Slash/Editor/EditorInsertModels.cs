using AIGuiders.Platform.IntermediateRepresentation.Command;
#nullable enable

namespace AIGuiders.Platform.CommandPlane.Editor;

/// <summary>1-based inclusive line range (CIDE ADR-0081 parity).</summary>
public readonly record struct EditorLineRange(int StartLine, int EndLine)
{
    public bool IsValid(int lineCount) =>
        StartLine >= 1 && EndLine >= StartLine && EndLine <= lineCount;
}

/// <summary>Character span in editor buffer (UTF-16 offsets like textarea).</summary>
public readonly record struct EditorSelectionSpan(int Start, int End)
{
    public int Length => End > Start ? End - Start : 0;
    public bool IsEmpty => Start == End;
}

public sealed record EditorTextEditResult(string Text, int SelectionStart, int SelectionEnd);

/// <summary>Markdown/HTML insert definition (slash id + wire payload).</summary>
public sealed record TextInsertFormatDefinition(
    string Id,
    string Path,
    string Help,
    string Category = "Format",
    string? Insert = null,
    string? HtmlInsert = null,
    string? WrapOpen = null,
    string? WrapClose = null,
    string? HtmlWrapOpen = null,
    string? HtmlWrapClose = null)
{
    public string WireClass =>
        WrapOpen is not null ? EditorWireClasses.FormatWrap : EditorWireClasses.FormatInsert;

    public string ArgTail => CommandArgTailPolicy.ImplicitSelection;
}

public sealed record TextDialectDefinition(
    string Id,
    string DisplayName,
    IReadOnlyList<TextInsertFormatDefinition> InsertFormats,
    bool Default = false,
    string ModeCommandPath = "/editor text markdown");

public sealed class TextInsertFormatDefinitionDto
{
    public required string Id { get; init; }
    public required string Path { get; init; }
    public required string Help { get; init; }
    public string Category { get; init; } = "Format";
    public string? Insert { get; init; }
    public string? HtmlInsert { get; init; }
    public string[]? Wrap { get; init; }
    public string[]? HtmlWrap { get; init; }
    public string WireClass { get; init; } = EditorWireClasses.FormatWrap;
    public string ArgTail { get; init; } = CommandArgTailPolicy.ImplicitSelection;
}

public sealed class TextDialectCapabilitiesDto
{
    public required string Id { get; init; }
    public required string DisplayName { get; init; }
    public bool Default { get; init; }
    public required IReadOnlyList<TextInsertFormatDefinitionDto> InsertCommands { get; init; }
    public string? ModeCommand { get; init; }
}

public static class EditorWireClasses
{
    public const string FormatWrap = "format_wrap";
    public const string FormatInsert = "format_insert";
    public const string IntChainColonSpace = "int_chain_colon_space";
}
