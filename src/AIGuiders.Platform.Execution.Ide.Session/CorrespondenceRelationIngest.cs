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

    public static IngestResult IngestReverseAnchors(IEnumerable<ReverseAnchor> anchors, SessionRuntime runtime)
    {
        ArgumentNullException.ThrowIfNull(anchors);
        ArgumentNullException.ThrowIfNull(runtime);

        var models = anchors.Select(static anchor => anchor.ToModel()).ToArray();
        var (updated, materialized, skipped) = CorrespondenceRelationOps.ingestReverseAnchors(models, runtime);
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
        if (bundle is null || bundle.ReverseAnchors.Length == 0)
            return new IngestResult(runtime, 0, 0);

        return IngestReverseAnchors(bundle.ReverseAnchors, runtime);
    }
}
