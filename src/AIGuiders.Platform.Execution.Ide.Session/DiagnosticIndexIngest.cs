#nullable enable

using AIGuiders.Platform.Modeling.Core.Identity;
using AIGuiders.Platform.Modeling.Ide.Session;
using AIGuiders.Platform.Modeling.LanguageIntelligence.Relations;
using AIGuiders.Platform.Modeling.Paths;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

namespace AIGuiders.Platform.Execution.Ide.Session;

/// <summary>
/// Execution ingest: LRC-shaped diagnostics → session diagnostic index via registry path→DocId (plan §2.4.2).
/// </summary>
public static class DiagnosticIndexIngest
{
    public sealed record LanguageDiagnostic(
        string Code,
        string Severity,
        string Message,
        string FilePath,
        TextSpan Span,
        IReadOnlyList<string>? Tags = null,
        string Language = "csharp");

    public sealed record IngestResult(SessionRuntime Runtime, int Ingested, int SkippedUnregistered);

    public static bool TryResolveDocId(string filePath, SessionRuntime runtime, out Identity<Document, NumericId> docId)
    {
        docId = default!;
        ArgumentNullException.ThrowIfNull(runtime);
        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        var logical = LogicalPath.Create(Path.GetFullPath(filePath.Trim()));
        var resolved = DocumentRegistryOps.resolvePath(logical, runtime.Registry);
        if (!FSharpOption<Identity<Document, NumericId>>.get_IsSome(resolved))
            return false;

        docId = resolved!.Value;
        return true;
    }

    public static IngestResult Ingest(IEnumerable<LanguageDiagnostic> diagnostics, SessionRuntime runtime)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);
        ArgumentNullException.ThrowIfNull(runtime);

        var incoming = new List<DiagnosticIndexOps.IncomingDiagnostic>();
        var skipped = 0;

        foreach (var diagnostic in diagnostics)
        {
            if (!TryResolveDocId(diagnostic.FilePath, runtime, out var docId))
            {
                skipped++;
                continue;
            }

            if (!runtime.Registry.ContainsKey(docId))
            {
                skipped++;
                continue;
            }

            var meta = runtime.Registry[docId];
            incoming.Add(new DiagnosticIndexOps.IncomingDiagnostic(
                diagnostic.Code,
                diagnostic.Severity,
                diagnostic.Message,
                docId,
                diagnostic.Span,
                ToFSharpList(diagnostic.Tags),
                diagnostic.Language,
                meta.SurfaceVersion));
        }

        var updated = DiagnosticIndexOps.ingest(incoming, runtime);
        return new IngestResult(updated, incoming.Count, skipped);
    }

    static FSharpList<string> ToFSharpList(IReadOnlyList<string>? tags)
    {
        if (tags is null || tags.Count == 0)
            return ListModule.Empty<string>();

        return ListModule.OfSeq(tags);
    }
}
