#nullable enable

namespace AIGuiders.Platform.CommandPlane.Editor;

/// <summary>Capabilities slice: text dialects + bundled editor-local slash commands.</summary>
public static class EditorSurfaceCatalog
{
    public static IReadOnlyList<TextDialectCapabilitiesDto> BuildTextDialects() =>
        [ToDto(MarkdownTextDialectCatalog.Markdown)];

    public static IReadOnlyList<SlashCommandDescriptor> BundledEditorLineCommands() =>
    [
        new SlashCommandDescriptor
        {
            Domain = "editor",
            Object = "line",
            Intent = "select",
            CommandId = "editor.line.select",
            Path = "/editor line select",
            Help = "Select line range (tail: start end or start:end)",
            Group = "Editor",
            ArgTail = SlashArgTailPolicy.ImplicitLineRange,
            ArgHint = "Line range — e.g. 5 10 or 5:10; empty uses current selection",
            Surfaces = ["editor-ccl", "editor-inline"],
            RequiredCapabilities = ["read"],
            Tier = "core",
        },
        new SlashCommandDescriptor
        {
            Domain = "editor",
            Object = "line",
            Intent = "delete",
            CommandId = "editor.line.delete",
            Path = "/editor line delete",
            Help = "Delete line range",
            Group = "Editor",
            ArgTail = SlashArgTailPolicy.ImplicitLineRange,
            ArgHint = "Line range — empty uses current selection lines",
            Surfaces = ["editor-ccl", "editor-inline"],
            RequiredCapabilities = ["write"],
            Tier = "core",
        },
    ];

    public static TextDialectCapabilitiesDto ToDto(TextDialectDefinition dialect) =>
        new()
        {
            Id = dialect.Id,
            DisplayName = dialect.DisplayName,
            Default = dialect.Default,
            ModeCommand = dialect.ModeCommandPath,
            InsertCommands = dialect.InsertFormats.Select(ToDto).ToArray(),
        };

    public static TextInsertFormatDefinitionDto ToDto(TextInsertFormatDefinition format) =>
        new()
        {
            Id = format.Id,
            Path = format.Path,
            Help = format.Help,
            Category = format.Category,
            Insert = format.Insert,
            HtmlInsert = format.HtmlInsert,
            Wrap = format.WrapOpen is not null && format.WrapClose is not null
                ? [format.WrapOpen, format.WrapClose]
                : null,
            HtmlWrap = format.HtmlWrapOpen is not null && format.HtmlWrapClose is not null
                ? [format.HtmlWrapOpen, format.HtmlWrapClose]
                : null,
            WireClass = format.WireClass,
            ArgTail = format.ArgTail,
        };
}
