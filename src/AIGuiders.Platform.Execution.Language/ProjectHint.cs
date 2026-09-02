namespace AIGuiders.Platform.Execution.Language;

/// <summary>Optional project/solution hint for backend resolution.</summary>
public sealed record ProjectHint(string? SolutionOrProjectPath, string? SessionDefaultLanguageId = null);
