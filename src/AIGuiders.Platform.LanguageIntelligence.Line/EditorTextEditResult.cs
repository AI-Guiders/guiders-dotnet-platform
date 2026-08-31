#nullable enable

namespace AIGuiders.Platform.LanguageIntelligence.Line;

public sealed record EditorTextEditResult(string Text, int SelectionStart, int SelectionEnd);
