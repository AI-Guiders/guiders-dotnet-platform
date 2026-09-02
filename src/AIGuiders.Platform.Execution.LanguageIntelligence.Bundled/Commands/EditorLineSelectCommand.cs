using AIGuiders.Platform.IntermediateRepresentation.Command;
#nullable enable

using AIGuiders.Platform.Execution.CommandPlane;
using AIGuiders.Platform.Execution.CommandPlane.Commands;
using AIGuiders.Platform.Execution.LanguageIntelligence.Bundled;
using AIGuiders.Platform.Execution.LanguageIntelligence.Edit;
using AIGuiders.Platform.Execution.LanguageIntelligence.Line;
using AIGuiders.Platform.Execution.LanguageIntelligence.Markup;

namespace AIGuiders.Platform.Execution.LanguageIntelligence.Bundled.Commands;

public sealed class EditorLineSelectCommand : PlatformCommand<EditorBufferContext>, ICatalogDescribed
{
    public const string Id = "editor.line.select";

    public override string CommandId => Id;

    public CommandDescriptor ToCommandDescriptor() => EditorCatalogProjections.EditorLineSelectDescriptor();

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
