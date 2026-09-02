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
