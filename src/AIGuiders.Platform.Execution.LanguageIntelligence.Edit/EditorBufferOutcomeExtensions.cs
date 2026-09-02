#nullable enable
using AIGuiders.Platform.Execution.CommandPlane.Commands;
using AIGuiders.Platform.Execution.LanguageIntelligence.Line;

namespace AIGuiders.Platform.Execution.LanguageIntelligence.Edit;

public static class EditorBufferOutcomeExtensions
{
    public static global::AIGuiders.Platform.Execution.CommandPlane.Commands.EditorBufferOutcome FromEdit(EditorTextEditResult edit) =>
        new()
        {
            Text = edit.Text,
            SelectionStart = edit.SelectionStart,
            SelectionEnd = edit.SelectionEnd,
        };
}
