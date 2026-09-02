using System.Text.Json;
using AIGuiders.Platform.Modeling.Language;

namespace AIGuiders.Platform.Execution.Language;

/// <summary>Wraps a host bridge that returns JSON payloads for LRC verbs.</summary>
public sealed class HostDelegatedLanguageBackend : ILanguageBackend
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly IHostLanguageBridge _bridge;

    public HostDelegatedLanguageBackend(IHostLanguageBridge bridge)
    {
        _bridge = bridge ?? throw new ArgumentNullException(nameof(bridge));
    }

    public string LanguageId => _bridge.LanguageId;

    public bool CanHandle(string path, ProjectHint hint) =>
        LanguagePathRules.ResolveLanguageId(path) == LanguageId;

    public async Task<DiagnosticsResult> GetDiagnosticsAsync(LanguageRequest req, CancellationToken ct)
    {
        var json = await _bridge.DispatchVerbAsync("diagnostics", req, ct).ConfigureAwait(false);
        return JsonSerializer.Deserialize<DiagnosticsResult>(json, JsonOptions)
            ?? new DiagnosticsResult { Diagnostics = [] };
    }

    public async Task<DocumentSymbolsResult> GetDocumentSymbolsAsync(LanguageRequest req, CancellationToken ct)
    {
        var json = await _bridge.DispatchVerbAsync("symbols", req, ct).ConfigureAwait(false);
        return JsonSerializer.Deserialize<DocumentSymbolsResult>(json, JsonOptions)
            ?? new DocumentSymbolsResult
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

    public async Task<LanguageNavigation?> GoToDefinitionAsync(LanguageRequest req, CancellationToken ct)
    {
        var json = await _bridge.DispatchVerbAsync("goto", req, ct).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(json) || json == "null")
            return null;

        return JsonSerializer.Deserialize<LanguageNavigation>(json, JsonOptions);
    }

    public async Task<FindUsagesResult> FindUsagesAsync(LanguageRequest req, CancellationToken ct)
    {
        var json = await _bridge.DispatchVerbAsync("find_usages", req, ct).ConfigureAwait(false);
        return JsonSerializer.Deserialize<FindUsagesResult>(json, JsonOptions)
            ?? new FindUsagesResult { References = [] };
    }

    public async Task<CompletionsResult> GetCompletionsAsync(LanguageRequest req, CancellationToken ct)
    {
        var json = await _bridge.DispatchVerbAsync("completions", req, ct).ConfigureAwait(false);
        return JsonSerializer.Deserialize<CompletionsResult>(json, JsonOptions)
            ?? new CompletionsResult { Items = [] };
    }

    public async Task<SymbolAtPositionResult?> GetSymbolAtPositionAsync(LanguageRequest req, CancellationToken ct)
    {
        var json = await _bridge.DispatchVerbAsync("symbol_at_position", req, ct).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(json) || json == "null")
            return null;

        return JsonSerializer.Deserialize<SymbolAtPositionResult>(json, JsonOptions);
    }

    public async Task<RenameSymbolResult> RenameSymbolAsync(RenameSymbolRequest req, CancellationToken ct)
    {
        var json = await _bridge.DispatchVerbAsync("rename", req.Request, ct).ConfigureAwait(false);
        return JsonSerializer.Deserialize<RenameSymbolResult>(json, JsonOptions)
            ?? new RenameSymbolResult
            {
                OldName = "",
                NewName = req.NewName,
                SymbolKind = "",
                Applied = false,
                Message = "",
                Files = [],
                Changes = [],
            };
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
