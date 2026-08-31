using AIGuiders.Platform.IntermediateRepresentation.Command;
#nullable enable
using AIGuiders.Platform.CommandPlane;
using AIGuiders.Platform.LanguageIntelligence.Bundled.Commands;
using AIGuiders.Platform.LanguageIntelligence.Edit;
using AIGuiders.Platform.LanguageIntelligence.Markup;

namespace AIGuiders.Platform.LanguageIntelligence.Bundled;

/// <summary>Slash catalog projections for bundled editor commands.</summary>
public static class EditorCatalogProjections
{
    public static CommandDescriptor ToCommandDescriptor(this TextInsertFormatDefinition format) =>
        CommandDescriptors.Describe(format.Id)
            .Domain("editor")
            .Object("format")
            .Intent(format.Id)
            .Path(format.Path.TrimStart('/'))
            .Help(format.Help)
            .Group(format.Category)
            .ArgTail(format.ArgTail)
            .Surfaces("editor-ccl", "editor-inline")
            .RequiredCapabilities("write")
            .Tier("core")
            .Build();

    public static CommandDescriptor EditorLineSelectDescriptor() =>
        CommandDescriptors.Describe(EditorLineSelectCommand.Id)
            .Domain("editor")
            .Object("line")
            .Intent("select")
            .Path("editor line select")
            .Help("Select line range (tail: start end or start:end)")
            .Group("Editor")
            .ArgTail(CommandArgTailPolicy.ImplicitLineRange)
            .ArgHint("Line range — e.g. 5 10 or 5:10; empty uses current selection")
            .Surfaces("editor-ccl", "editor-inline")
            .RequiredCapabilities("read")
            .Tier("core")
            .Build();

    public static CommandDescriptor EditorLineDeleteDescriptor() =>
        CommandDescriptors.Describe(EditorLineDeleteCommand.Id)
            .Domain("editor")
            .Object("line")
            .Intent("delete")
            .Path("editor line delete")
            .Help("Delete line range")
            .Group("Editor")
            .ArgTail(CommandArgTailPolicy.ImplicitLineRange)
            .ArgHint("Line range — empty uses current selection lines")
            .Surfaces("editor-ccl", "editor-inline")
            .RequiredCapabilities("write")
            .Tier("core")
            .Build();
}
