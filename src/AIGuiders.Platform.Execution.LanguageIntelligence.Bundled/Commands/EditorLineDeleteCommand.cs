using AIGuiders.Platform.IntermediateRepresentation.Command;
#nullable enable

using AIGuiders.Platform.Execution.CommandPlane;
using AIGuiders.Platform.Execution.CommandPlane.Commands;
using AIGuiders.Platform.Execution.LanguageIntelligence.Bundled;
using AIGuiders.Platform.Execution.LanguageIntelligence.Edit;
using AIGuiders.Platform.Execution.LanguageIntelligence.Line;

namespace AIGuiders.Platform.Execution.LanguageIntelligence.Bundled.Commands;

public sealed class EditorLineDeleteCommand : PlatformCommand<EditorBufferContext>, ICatalogDescribed
{
    public const string Id = "editor.line.delete";

    public override string CommandId => Id;

    public CommandDescriptor ToCommandDescriptor() => EditorCatalogProjections.EditorLineDeleteDescriptor();

    protected override CommandOutcome Execute(EditorBufferContext context)
    {
        if (!EditorLineCommandSupport.TryResolveLineRange(context, out var range))
            return CommandOutcome.Fail("Line range is required.");

        var edit = EditorLineTextOps.DeleteLineRange(context.Text, range);
        return CommandOutcome.Ok(EditorBufferOutcomeExtensions.FromEdit(edit));
    }
}
