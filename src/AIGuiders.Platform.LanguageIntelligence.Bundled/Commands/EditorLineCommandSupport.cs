#nullable enable

using AIGuiders.Platform.LanguageIntelligence.Edit;
using AIGuiders.Platform.LanguageIntelligence.Line;

namespace AIGuiders.Platform.LanguageIntelligence.Bundled.Commands;

internal static class EditorLineCommandSupport
{
    public static bool TryResolveLineRange(EditorBufferContext context, out EditorLineRange range)
    {
        if (EditorLineRangeParser.TryParse(context.ArgsTail, out range))
            return true;

        range = EditorLineTextOps.LineRangeForSelection(context.Text, context.Selection);
        return range.StartLine > 0;
    }
}
