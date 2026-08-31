using AIGuiders.Platform.IntermediateRepresentation.Command;
#nullable enable

using AIGuiders.Platform.CommandPlane;
using AIGuiders.Platform.CommandPlane.Commands;
using AIGuiders.Platform.LanguageIntelligence.Bundled;
using AIGuiders.Platform.LanguageIntelligence.Edit;
using AIGuiders.Platform.LanguageIntelligence.Line;

namespace AIGuiders.Platform.LanguageIntelligence.Bundled.Commands;

public sealed class EditorLineDeleteCommand : PlatformCommand<EditorBufferContext>, ICatalogDescribed
{
    public const string Id = "editor.line.delete";

    public override string CommandId => Id;

    public CommandDescriptor ToSlashDescriptor() => EditorCatalogProjections.EditorLineDeleteDescriptor();

    protected override CommandOutcome Execute(EditorBufferContext context)
    {
        if (!EditorLineCommandSupport.TryResolveLineRange(context, out var range))
            return CommandOutcome.Fail("Line range is required.");

        var edit = EditorLineTextOps.DeleteLineRange(context.Text, range);
        return CommandOutcome.Ok(EditorBufferOutcomeExtensions.FromEdit(edit));
    }
}
