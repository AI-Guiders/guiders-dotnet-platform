#nullable enable
using AIGuiders.Platform.CommandPlane.Commands;
using AIGuiders.Platform.LanguageIntelligence.Line;

namespace AIGuiders.Platform.LanguageIntelligence.Edit;

public static class EditorBufferOutcomeExtensions
{
    public static global::AIGuiders.Platform.CommandPlane.Commands.EditorBufferOutcome FromEdit(EditorTextEditResult edit) =>
        new()
        {
            Text = edit.Text,
            SelectionStart = edit.SelectionStart,
            SelectionEnd = edit.SelectionEnd,
        };
}
