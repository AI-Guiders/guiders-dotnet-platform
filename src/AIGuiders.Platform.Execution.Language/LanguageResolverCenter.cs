using AIGuiders.Platform.Modeling.Language;
using AIGuiders.Platform.Execution.Ide.Session;

namespace AIGuiders.Platform.Execution.Language;

/// <summary>Federation gateway: resolve backends and dispatch IDE language verbs.</summary>
public sealed class LanguageResolverCenter
{
    private readonly IReadOnlyList<ILanguageBackend> _backends;
    private readonly ILanguageActivationCatalog? _activation;

    public LanguageResolverCenter(IEnumerable<ILanguageBackend> backends)
        : this(backends, activation: null)
    {
    }

    public LanguageResolverCenter(
        IEnumerable<ILanguageBackend> backends,
        ILanguageActivationCatalog? activation)
    {
        _backends = backends?.ToList() ?? throw new ArgumentNullException(nameof(backends));
        _activation = activation;
    }

    public IReadOnlyList<ILanguageBackend> Backends => _backends;

    public ILanguageActivationCatalog? Activation => _activation;

    public ILanguageBackend? Resolve(string path, ProjectHint? hint = null)
    {
        hint ??= new ProjectHint { SolutionOrProjectPath = "", SessionDefaultLanguageId = "" };

        foreach (var backend in _backends)
        {
            if (backend.CanHandle(path, hint))
                return backend;
        }

        var languageId = ResolveLanguageId(path);
        if (string.IsNullOrEmpty(languageId))
            return null;

        return _backends.FirstOrDefault(b =>
            b.LanguageId.Equals(languageId, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<DiagnosticsResult> DispatchDiagnosticsAsync(
        LanguageRequest req,
        CancellationToken ct = default)
    {
        var backend = Resolve(req.FilePath, Hint(req.SolutionOrProjectPath));
        if (backend is null)
            return new DiagnosticsResult { Diagnostics = [] };

        var result = await backend.GetDiagnosticsAsync(req, ct).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(req.SolutionOrProjectPath))
            FederationSessionRuntime.TryRefreshDiagnosticIndex(req.SolutionOrProjectPath, result);

        return result;
    }

    public async Task<DocumentSymbolsResult> DispatchDocumentSymbolsAsync(
        LanguageRequest req,
        CancellationToken ct = default)
    {
        var backend = Resolve(req.FilePath, Hint(req.SolutionOrProjectPath));
        if (backend is null)
        {
            return new DocumentSymbolsResult
            {
                Root = new LanguageSymbol
                {
                    Name = Path.GetFileName(req.FilePath),
                    Kind = "file",
                    Span = EmptySpan(req.FilePath),
                    Container = "",
                    Children = [],
                },
            };
        }

        return await backend.GetDocumentSymbolsAsync(req, ct).ConfigureAwait(false);
    }

    public async Task<LanguageNavigation?> DispatchGoToDefinitionAsync(
        LanguageRequest req,
        CancellationToken ct = default)
    {
        var backend = Resolve(req.FilePath, Hint(req.SolutionOrProjectPath));
        if (backend is null)
            return null;

        return await backend.GoToDefinitionAsync(req, ct).ConfigureAwait(false);
    }

    public async Task<FindUsagesResult> DispatchFindUsagesAsync(
        LanguageRequest req,
        CancellationToken ct = default)
    {
        var backend = Resolve(req.FilePath, Hint(req.SolutionOrProjectPath));
        if (backend is null)
            return new FindUsagesResult { References = [] };

        return await backend.FindUsagesAsync(req, ct).ConfigureAwait(false);
    }

    public async Task<CompletionsResult> DispatchCompletionsAsync(
        LanguageRequest req,
        CancellationToken ct = default)
    {
        var backend = Resolve(req.FilePath, Hint(req.SolutionOrProjectPath));
        if (backend is null)
            return new CompletionsResult { Items = [] };

        return await backend.GetCompletionsAsync(req, ct).ConfigureAwait(false);
    }

    public async Task<SymbolAtPositionResult?> DispatchSymbolAtPositionAsync(
        LanguageRequest req,
        CancellationToken ct = default)
    {
        var backend = Resolve(req.FilePath, Hint(req.SolutionOrProjectPath));
        if (backend is null)
            return null;

        return await backend.GetSymbolAtPositionAsync(req, ct).ConfigureAwait(false) switch
        {
            { Name: { Length: > 0 } } symbol => symbol,
            _ => null,
        };
    }

    public async Task<RenameSymbolResult> DispatchRenameSymbolAsync(
        RenameSymbolRequest req,
        CancellationToken ct = default)
    {
        var backend = Resolve(req.Request.FilePath, Hint(req.Request.SolutionOrProjectPath));
        if (backend is null)
        {
            return new RenameSymbolResult
            {
                OldName = "",
                NewName = req.NewName,
                SymbolKind = "",
                Applied = false,
                Message = "No language backend resolved.",
                Files = [],
                Changes = [],
            };
        }

        return await backend.RenameSymbolAsync(req, ct).ConfigureAwait(false);
    }

    private string? ResolveLanguageId(string path)
    {
        if (_activation is not null)
        {
            var activated = _activation.ResolveLanguageId(path);
            if (!string.IsNullOrEmpty(activated))
                return activated;
        }

        return LanguagePathRules.ResolveLanguageId(path);
    }

    private static ProjectHint Hint(string? solutionOrProjectPath) =>
        new() { SolutionOrProjectPath = solutionOrProjectPath ?? "", SessionDefaultLanguageId = "" };

    private static SourceSpan EmptySpan(string path) =>
        new()
        {
            Path = path,
            Line = 1,
            Column = 1,
            EndLine = 1,
            EndColumn = 1,
        };
}
