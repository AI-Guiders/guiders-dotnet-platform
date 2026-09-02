#nullable enable

namespace AIGuiders.Platform.Execution.Sources;

/// <summary>Loads a document from a product-owned backend (code, file, DB, HTTP).</summary>
public interface ISource<out T>
{
    /// <summary>Stable id for diagnostics and merge tracing (e.g. <c>file:workspace.toml</c>).</summary>
    string SourceId { get; }

    T Load();
}
