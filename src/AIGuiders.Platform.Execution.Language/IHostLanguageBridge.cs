namespace AIGuiders.Platform.Execution.Language;

/// <summary>Host-delegated bridge for planet-local language engines (Roslyn, tsserver, …).</summary>
public interface IHostLanguageBridge
{
    string LanguageId { get; }

    Task<string> DispatchVerbAsync(string verb, LanguageRequest request, CancellationToken cancellationToken);
}
