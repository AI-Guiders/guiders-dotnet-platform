using System.Threading;
using System.Threading.Tasks;
using AIGuiders.Platform.Modeling.Language;

namespace AIGuiders.Platform.Language.CSharp;

/// <summary>Activation/LRC stub until Roslyn adapter ships as planet extension.</summary>
public sealed class CsharpLanguageBackend : ILanguageBackend
{
    public string LanguageId => Modeling.Language.LanguageIds.Csharp;

    public bool CanHandle(string path, ProjectHint hint) =>
        AIGuiders.Platform.Execution.Language.LanguagePathRules.ResolveLanguageId(path)
        == Modeling.Language.LanguageIds.Csharp;

    public Task<DiagnosticsResult> GetDiagnosticsAsync(LanguageRequest req, CancellationToken ct) =>
        Task.FromResult(new DiagnosticsResult { Diagnostics = [] });

    public Task<DocumentSymbolsResult> GetDocumentSymbolsAsync(LanguageRequest req, CancellationToken ct) =>
        Task.FromResult(EmptySymbols(req.FilePath));

    public Task<LanguageNavigation> GoToDefinitionAsync(LanguageRequest req, CancellationToken ct) =>
        Task.FromResult(EmptyNavigation(req.FilePath));

    public Task<FindUsagesResult> FindUsagesAsync(LanguageRequest req, CancellationToken ct) =>
        Task.FromResult(new FindUsagesResult { References = [] });

    public Task<CompletionsResult> GetCompletionsAsync(LanguageRequest req, CancellationToken ct) =>
        Task.FromResult(new CompletionsResult { Items = [] });

    public Task<SymbolAtPositionResult> GetSymbolAtPositionAsync(LanguageRequest req, CancellationToken ct) =>
        Task.FromResult(EmptySymbol(req.FilePath));

    public Task<RenameSymbolResult> RenameSymbolAsync(RenameSymbolRequest req, CancellationToken ct) =>
        Task.FromResult(new RenameSymbolResult
        {
            OldName = "",
            NewName = req.NewName,
            SymbolKind = "",
            Applied = false,
            Message = "",
            Files = [],
            Changes = [],
        });

    static DocumentSymbolsResult EmptySymbols(string path) => new()
    {
        Root = new LanguageSymbol
        {
            Name = Path.GetFileName(path),
            Kind = "file",
            Span = EmptySpan(path),
            Container = "",
            Children = [],
        },
    };

    static LanguageNavigation EmptyNavigation(string path) => new()
    {
        Definition = EmptySpan(path),
        Declarations = [EmptySpan(path)],
    };

    static SymbolAtPositionResult EmptySymbol(string path) => new()
    {
        Kind = "",
        Name = "",
        QualifiedName = "",
        Span = EmptySpan(path),
    };

    static SourceSpan EmptySpan(string path) => new()
    {
        Path = path,
        Line = 1,
        Column = 1,
        EndLine = 1,
        EndColumn = 1,
    };
}
