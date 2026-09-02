namespace AIGuiders.Platform.Execution.Language;

/// <summary>LRC rename verb request envelope.</summary>
public sealed record RenameSymbolRequest(
    LanguageRequest Request,
    string NewName,
    bool Apply = false);
