using AIGuiders.Platform.IntermediateRepresentation.Command;
#nullable enable

using AIGuiders.Platform.CommandPlane;
using AIGuiders.Platform.CommandPlane.Commands;
using AIGuiders.Platform.LanguageIntelligence.Edit;
using AIGuiders.Platform.LanguageIntelligence.Markup;

namespace AIGuiders.Platform.LanguageIntelligence.Bundled.Commands;

public sealed class EditorFormatInsertCommand(TextInsertFormatDefinition format)
    : PlatformCommand<EditorBufferContext>, ICatalogDescribed
{
    public override string CommandId => format.Id;

    public CommandDescriptor ToCommandDescriptor() => format.ToCommandDescriptor();

    protected override CommandOutcome Execute(EditorBufferContext context)
    {
        var edit = EditorTextTransform.ApplyFormat(
            context.Text,
            context.Selection,
            format,
            context.TextMode);
        return CommandOutcome.Ok(EditorBufferOutcomeExtensions.FromEdit(edit));
    }
}
