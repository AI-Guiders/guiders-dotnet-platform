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

    /// <summary>Scan session registry paths for CRS doc→code witnesses (plan product wiring @ Open).</summary>
    public static IngestResult IngestFromRegistry(SessionRuntime runtime, string? workspaceRootHint = null)
    {
        ArgumentNullException.ThrowIfNull(runtime);

        var updated = runtime;
        var materialized = 0;
        var skipped = 0;
        var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var meta in runtime.Registry.Values)
        {
            var path = meta.Path.Value;
            if (string.IsNullOrWhiteSpace(path) || !seenPaths.Add(path))
                continue;

            string abs;
            try
            {
                abs = Path.GetFullPath(path);
            }
            catch (ArgumentException)
            {
                skipped++;
                continue;
            }

            if (!File.Exists(abs))
                continue;

            var result = TryIngestForFile(updated, abs, workspaceRootHint);
            updated = result.Runtime;
            materialized += result.Materialized;
            skipped += result.Skipped;
        }

        return new IngestResult(updated, materialized, skipped);
    }
}
