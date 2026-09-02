#nullable enable

using AIGuiders.Platform.Execution.CommandPlane.Commands;
using AIGuiders.Platform.Execution.LanguageIntelligence.Line;

namespace AIGuiders.Platform.Execution.LanguageIntelligence.Edit;

/// <summary>Invocation context for editor buffer commands (selection snapshot + args tail).</summary>
public sealed class EditorBufferContext : ICommandContext
{
    public required string Text { get; init; }
    public EditorSelectionSpan Selection { get; init; }
    public string TextMode { get; init; } = "markdown";
    public string? ArgsTail { get; init; }
}
