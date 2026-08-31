using AIGuiders.Platform.CommandPlane;
using AIGuiders.Platform.IntermediateRepresentation.Command;
#nullable enable

using AIGuiders.Platform.LanguageIntelligence.Edit;
using AIGuiders.Platform.LanguageIntelligence.Markup;

namespace AIGuiders.Platform.LanguageIntelligence.Bundled;

/// <summary>Capabilities slice: text dialects + bundled editor-local slash commands.</summary>
/// <remarks>Deprecated quarry — migrate to <c>LanguageIntelligence.*</c> per GUIDERS-ADR-0025 Phase 1.</remarks>
public static class EditorSurfaceCatalog
{
    public static IReadOnlyList<TextDialectCapabilitiesDto> BuildTextDialects() =>
        [ToDto(MarkdownTextDialectCatalog.Markdown)];

    public static IReadOnlyList<CommandDescriptor> BundledEditorLineCommands() =>
        RegistryCatalogBuilder.CollectDescriptors(
            EditorCommandRegistry.BundledRegistry,
            descriptor => descriptor.CommandId.StartsWith("editor.line.", StringComparison.OrdinalIgnoreCase));

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
