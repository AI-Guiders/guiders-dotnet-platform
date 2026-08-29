#nullable enable

using AIGuiders.Platform.CommandPlane.Commands;

namespace AIGuiders.Platform.CommandPlane.Editor.Commands;

public sealed class EditorLineSelectCommand : PlatformCommand<EditorBufferContext>, ICatalogDescribed
{
    public const string Id = "editor.line.select";

    public override string CommandId => Id;

    public SlashCommandDescriptor ToSlashDescriptor() => EditorCatalogProjections.EditorLineSelectDescriptor();

    protected override CommandOutcome Execute(EditorBufferContext context)
    {
        if (!EditorLineCommandSupport.TryResolveLineRange(context, out var range))
            return CommandOutcome.Fail("Line range is required.");

        var span = EditorLineTextOps.SelectionSpanForLineRange(context.Text, range);
        return CommandOutcome.Ok(new EditorBufferOutcome
        {
            Text = context.Text,
            SelectionStart = span.Start,
            SelectionEnd = span.End,
        });
    }
}
