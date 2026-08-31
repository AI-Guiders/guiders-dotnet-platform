using AIGuiders.Platform.IntermediateRepresentation.Command;
#nullable enable

using AIGuiders.Platform.CommandPlane;
using AIGuiders.Platform.CommandPlane.Commands;
using AIGuiders.Platform.LanguageIntelligence.Bundled;
using AIGuiders.Platform.LanguageIntelligence.Edit;
using AIGuiders.Platform.LanguageIntelligence.Line;
using AIGuiders.Platform.LanguageIntelligence.Markup;

namespace AIGuiders.Platform.LanguageIntelligence.Bundled.Commands;

public sealed class EditorLineSelectCommand : PlatformCommand<EditorBufferContext>, ICatalogDescribed
{
    public const string Id = "editor.line.select";

    public override string CommandId => Id;

    public CommandDescriptor ToSlashDescriptor() => EditorCatalogProjections.EditorLineSelectDescriptor();

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
