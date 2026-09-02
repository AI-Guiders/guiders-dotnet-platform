namespace AIGuiders.Platform.Execution.Language;

/// <summary>Gateway request for LRC verb dispatch.</summary>
public sealed record LanguageRequest(
    string FilePath,
    int Line,
    int Column,
    string? SourceText = null,
    string? SolutionOrProjectPath = null);
