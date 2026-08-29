#nullable enable

using AIGuiders.Platform.CommandPlane.Commands;

namespace AIGuiders.Platform.CommandPlane.Editor.Commands;

public sealed class EditorFormatInsertCommand(TextInsertFormatDefinition format)
    : PlatformCommand<EditorBufferContext>, ICatalogDescribed
{
    public override string CommandId => format.Id;

    public SlashCommandDescriptor ToSlashDescriptor() => format.ToSlashDescriptor();

    protected override CommandOutcome Execute(EditorBufferContext context)
    {
        var edit = EditorTextTransform.ApplyFormat(
            context.Text,
            context.Selection,
            format,
            context.TextMode);
        return CommandOutcome.Ok(EditorBufferOutcome.FromEdit(edit));
    }
}
