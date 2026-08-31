#nullable enable

using AIGuiders.Platform.CommandPlane.Commands;
using AIGuiders.Platform.LanguageIntelligence.Line;

namespace AIGuiders.Platform.LanguageIntelligence.Edit;

/// <summary>Invocation context for editor buffer commands (selection snapshot + args tail).</summary>
public sealed class EditorBufferContext : ICommandContext
{
    public required string Text { get; init; }
    public EditorSelectionSpan Selection { get; init; }
    public string TextMode { get; init; } = "markdown";
    public string? ArgsTail { get; init; }
}
