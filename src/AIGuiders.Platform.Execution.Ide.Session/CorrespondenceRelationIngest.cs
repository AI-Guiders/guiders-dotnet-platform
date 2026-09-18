#nullable enable

using AIGuiders.Platform.Execution.Documentation.Correspondence;
using AIGuiders.Platform.Modeling.Ide.Session;

namespace AIGuiders.Platform.Execution.Ide.Session;

/// <summary>
/// CRS reverse anchors → session graph correspondence relations (plan Phase 2 @ federation boundary).
/// </summary>
public static class CorrespondenceRelationIngest
{
    public sealed record IngestResult(SessionRuntime Runtime, int Materialized, int Skipped);

    public static IngestResult IngestDocToCodeWitnesses(IEnumerable<DocToCodeWitness> witnesses, SessionRuntime runtime)
    {
        ArgumentNullException.ThrowIfNull(witnesses);
        ArgumentNullException.ThrowIfNull(runtime);

        var models = witnesses.Select(static witness => witness.ToModel()).ToArray();
        var (updated, materialized, skipped) = CorrespondenceRelationOps.ingestDocToCodeWitnesses(models, runtime);
        return new IngestResult(updated, materialized, skipped);
    }

    public static IngestResult TryIngestForFile(
        SessionRuntime runtime,
        string absoluteFilePath,
        string? workspaceRootHint = null)
    {
        ArgumentNullException.ThrowIfNull(runtime);
        ArgumentException.ThrowIfNullOrWhiteSpace(absoluteFilePath);

        var bundle = CorrespondenceResolver.TryResolve(absoluteFilePath, workspaceRootHint);
        if (bundle is null || bundle.DocToCodeWitnesses.Length == 0)
            return new IngestResult(runtime, 0, 0);

        return IngestDocToCodeWitnesses(bundle.DocToCodeWitnesses, runtime);
    }
}
