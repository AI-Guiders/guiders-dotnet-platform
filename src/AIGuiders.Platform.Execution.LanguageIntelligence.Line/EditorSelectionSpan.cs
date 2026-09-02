#nullable enable

namespace AIGuiders.Platform.Execution.LanguageIntelligence.Line;

/// <summary>Character span in editor buffer (UTF-16 offsets like textarea).</summary>
public readonly record struct EditorSelectionSpan(int Start, int End)
{
    public int Length => End > Start ? End - Start : 0;
    public bool IsEmpty => Start == End;
}
