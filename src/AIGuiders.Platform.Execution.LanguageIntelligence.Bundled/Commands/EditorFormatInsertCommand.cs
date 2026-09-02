using AIGuiders.Platform.IntermediateRepresentation.Command;
#nullable enable

using AIGuiders.Platform.Execution.CommandPlane;
using AIGuiders.Platform.Execution.CommandPlane.Commands;
using AIGuiders.Platform.Execution.LanguageIntelligence.Edit;
using AIGuiders.Platform.Execution.LanguageIntelligence.Markup;

namespace AIGuiders.Platform.Execution.LanguageIntelligence.Bundled.Commands;

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
