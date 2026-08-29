#nullable enable

using AIGuiders.Platform.CommandPlane.Commands;

namespace AIGuiders.Platform.CommandPlane.Editor.Commands;

public sealed class EditorLineDeleteCommand : PlatformCommand<EditorBufferContext>, ICatalogDescribed
{
    public const string Id = "editor.line.delete";

    public override string CommandId => Id;

    public SlashCommandDescriptor ToSlashDescriptor() => EditorCatalogProjections.EditorLineDeleteDescriptor();

    protected override CommandOutcome Execute(EditorBufferContext context)
    {
        if (!EditorLineCommandSupport.TryResolveLineRange(context, out var range))
            return CommandOutcome.Fail("Line range is required.");

        var edit = EditorLineTextOps.DeleteLineRange(context.Text, range);
        return CommandOutcome.Ok(EditorBufferOutcomeExtensions.FromEdit(edit));
    }
}
