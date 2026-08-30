#nullable enable
using AIGuiders.Platform.CommandPlane.Commands;

namespace AIGuiders.Platform.CommandPlane.Editor;

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
