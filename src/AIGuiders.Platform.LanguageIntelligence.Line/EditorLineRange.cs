using AIGuiders.Platform.IntermediateRepresentation.Command;
#nullable enable

namespace AIGuiders.Platform.LanguageIntelligence.Line;

/// <summary>1-based inclusive line range (CIDE ADR-0081 parity).</summary>
public readonly record struct EditorLineRange(int StartLine, int EndLine)
{
    public bool IsValid(int lineCount) =>
        StartLine >= 1 && EndLine >= StartLine && EndLine <= lineCount;
}