using AIGuiders.Platform.Modeling.Language;

namespace AIGuiders.Platform.Execution.Language;

/// <summary>Planet-local or in-process language service backend.</summary>
public interface ILanguageBackend
{
    string LanguageId { get; }

    bool CanHandle(string path, ProjectHint hint);

    Task<DiagnosticsResult> GetDiagnosticsAsync(LanguageRequest req, CancellationToken ct);

    Task<DocumentSymbolsResult> GetDocumentSymbolsAsync(LanguageRequest req, CancellationToken ct);

    Task<LanguageNavigation?> GoToDefinitionAsync(LanguageRequest req, CancellationToken ct);

    Task<FindUsagesResult> FindUsagesAsync(LanguageRequest req, CancellationToken ct);

    Task<CompletionsResult> GetCompletionsAsync(LanguageRequest req, CancellationToken ct);

    Task<SymbolAtPositionResult?> GetSymbolAtPositionAsync(LanguageRequest req, CancellationToken ct);

    Task<RenameSymbolResult> RenameSymbolAsync(RenameSymbolRequest req, CancellationToken ct);
}
