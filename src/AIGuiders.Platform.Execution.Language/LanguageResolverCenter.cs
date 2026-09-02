using AIGuiders.Platform.Modeling.Language;

namespace AIGuiders.Platform.Execution.Language;

/// <summary>Federation gateway: resolve backends and dispatch IDE language verbs.</summary>
public sealed class LanguageResolverCenter
{
    private readonly IReadOnlyList<ILanguageBackend> _backends;

    public LanguageResolverCenter(IEnumerable<ILanguageBackend> backends)
    {
        _backends = backends?.ToList() ?? throw new ArgumentNullException(nameof(backends));
    }

    public IReadOnlyList<ILanguageBackend> Backends => _backends;

    public ILanguageBackend? Resolve(string path, ProjectHint? hint = null)
    {
        hint ??= new ProjectHint(null);

        foreach (var backend in _backends)
        {
            if (backend.CanHandle(path, hint))
                return backend;
        }

        var languageId = LanguagePathRules.ResolveLanguageId(path);
        if (languageId is null)
            return null;

        return _backends.FirstOrDefault(b =>
            b.LanguageId.Equals(languageId, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<DiagnosticsResult> DispatchDiagnosticsAsync(
        LanguageRequest req,
        CancellationToken ct = default)
    {
        var backend = Resolve(req.FilePath, new ProjectHint(req.SolutionOrProjectPath));
        if (backend is null)
            return new DiagnosticsResult { Diagnostics = [] };

        return await backend.GetDiagnosticsAsync(req, ct).ConfigureAwait(false);
    }

    public async Task<DocumentSymbolsResult> DispatchDocumentSymbolsAsync(
        LanguageRequest req,
        CancellationToken ct = default)
    {
        var backend = Resolve(req.FilePath, new ProjectHint(req.SolutionOrProjectPath));
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
        var backend = Resolve(req.FilePath, new ProjectHint(req.SolutionOrProjectPath));
        if (backend is null)
            return null;

        return await backend.GoToDefinitionAsync(req, ct).ConfigureAwait(false);
    }

    public async Task<FindUsagesResult> DispatchFindUsagesAsync(
        LanguageRequest req,
        CancellationToken ct = default)
    {
        var backend = Resolve(req.FilePath, new ProjectHint(req.SolutionOrProjectPath));
        if (backend is null)
            return new FindUsagesResult { References = [] };

        return await backend.FindUsagesAsync(req, ct).ConfigureAwait(false);
    }

    public async Task<CompletionsResult> DispatchCompletionsAsync(
        LanguageRequest req,
        CancellationToken ct = default)
    {
        var backend = Resolve(req.FilePath, new ProjectHint(req.SolutionOrProjectPath));
        if (backend is null)
            return new CompletionsResult { Items = [] };

        return await backend.GetCompletionsAsync(req, ct).ConfigureAwait(false);
    }

    public async Task<SymbolAtPositionResult?> DispatchSymbolAtPositionAsync(
        LanguageRequest req,
        CancellationToken ct = default)
    {
        var backend = Resolve(req.FilePath, new ProjectHint(req.SolutionOrProjectPath));
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
        var backend = Resolve(req.Request.FilePath, new ProjectHint(req.Request.SolutionOrProjectPath));
        if (backend is null)
        {
            return new RenameSymbolResult
            {
                OldName = "",
                NewName = req.NewName,
                SymbolKind = "",
                Applied = false,
                Files = [],
                Changes = [],
            };
        }

        return await backend.RenameSymbolAsync(req, ct).ConfigureAwait(false);
    }

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
