#nullable enable

using AIGuiders.Platform.CommandPlane.Commands;

namespace AIGuiders.Platform.CommandPlane.Editor.Commands;

public sealed class EditorLineDeleteCommand : PlatformCommand<EditorBufferContext>
{
    public const string Id = "editor.line.delete";

    public override string CommandId => Id;

    protected override CommandOutcome Execute(EditorBufferContext context)
    {
        if (!EditorLineCommandSupport.TryResolveLineRange(context, out var range))
            return CommandOutcome.Fail("Line range is required.");

        var edit = EditorLineTextOps.DeleteLineRange(context.Text, range);
        return CommandOutcome.Ok(EditorBufferOutcome.FromEdit(edit));
    }
}
