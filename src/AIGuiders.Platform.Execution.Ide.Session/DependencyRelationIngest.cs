#nullable enable

using AIGuiders.Platform.Execution.Language.CSharp.Relations;
using AIGuiders.Platform.Modeling.Ide.Session;
using AIGuiders.Platform.Modeling.LanguageIntelligence.Relations;
using Microsoft.FSharp.Collections;

namespace AIGuiders.Platform.Execution.Ide.Session;

/// <summary>
/// Execution ingest: session <c>Contents</c> → Roslyn E_dep Uses → graph <c>Relations</c> (plan §2.3).
/// </summary>
public static class DependencyRelationIngest
{
    public sealed record IngestResult(SessionRuntime Runtime, int Ingested, int SkippedNonCs);

    public static IngestResult IngestFromContents(SessionRuntime runtime)
    {
        ArgumentNullException.ThrowIfNull(runtime);

        var before = runtime.Session.Graph.Relations.Length;
        var incoming = new List<Relation>();
        var skippedNonCs = 0;

        foreach (var docId in runtime.Contents.Keys)
        {
            if (!runtime.Registry.TryGetValue(docId, out var meta))
                continue;

            var path = meta.Path.Value;
            if (!path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            {
                skippedNonCs++;
                continue;
            }

            var text = runtime.Contents[docId].text;
            incoming.AddRange(RoslynDependencyRelationIngest.IngestUsesFromSource(path, text, meta.Owner));
        }

        var updated = DependencyRelationOps.ingest(ListModule.OfSeq(incoming), runtime);
        var ingested = updated.Session.Graph.Relations.Length - before;
        return new IngestResult(updated, ingested, skippedNonCs);
    }
}
