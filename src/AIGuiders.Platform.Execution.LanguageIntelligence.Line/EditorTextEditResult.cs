#nullable enable

namespace AIGuiders.Platform.Execution.LanguageIntelligence.Line;

public sealed record EditorTextEditResult(string Text, int SelectionStart, int SelectionEnd);
