#nullable enable

using AIGuiders.Platform.Modeling.Core.Identity;
using AIGuiders.Platform.Modeling.Ide.Session;
using AIGuiders.Platform.Modeling.Language;
using AIGuiders.Platform.Modeling.LanguageIntelligence.Relations;
using AIGuiders.Platform.Modeling.Paths;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using ModelingLanguageDiagnostic = AIGuiders.Platform.Modeling.Language.LanguageDiagnostic;

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
            if (!TryBuildIncoming(diagnostic, runtime, out var incomingDiagnostic))
            {
                skipped++;
                continue;
            }

            incoming.Add(incomingDiagnostic);
        }

        var updated = DiagnosticIndexOps.ingest(incoming, runtime);
        return new IngestResult(updated, incoming.Count, skipped);
    }

    public static IngestResult RefreshLrc(IEnumerable<ModelingLanguageDiagnostic> diagnostics, SessionRuntime runtime) =>
        Refresh(MapFromLrc(diagnostics), runtime);

    public static IngestResult Refresh(IEnumerable<LanguageDiagnostic> diagnostics, SessionRuntime runtime)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);
        ArgumentNullException.ThrowIfNull(runtime);

        var incoming = new List<DiagnosticIndexOps.IncomingDiagnostic>();
        var skipped = 0;

        foreach (var diagnostic in diagnostics)
        {
            if (!TryBuildIncoming(diagnostic, runtime, out var incomingDiagnostic))
            {
                skipped++;
                continue;
            }

            incoming.Add(incomingDiagnostic);
        }

        var updated = DiagnosticIndexOps.refresh(incoming, runtime);
        return new IngestResult(updated, incoming.Count, skipped);
    }

    public static IEnumerable<LanguageDiagnostic> MapFromLrc(IEnumerable<ModelingLanguageDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);

        foreach (var diagnostic in diagnostics)
        {
            if (string.IsNullOrWhiteSpace(diagnostic.Span?.Path))
                continue;

            yield return new LanguageDiagnostic(
                string.IsNullOrWhiteSpace(diagnostic.Id) ? "IDE0001" : diagnostic.Id.Trim(),
                WireSeverity(diagnostic.Severity),
                diagnostic.Message ?? "",
                diagnostic.Span.Path,
                new TextSpan(
                    diagnostic.Span.Line,
                    diagnostic.Span.Column,
                    diagnostic.Span.EndLine,
                    diagnostic.Span.EndColumn),
                diagnostic.Tags,
                string.IsNullOrWhiteSpace(diagnostic.Language) ? "csharp" : diagnostic.Language);
        }
    }

    static bool TryBuildIncoming(
        LanguageDiagnostic diagnostic,
        SessionRuntime runtime,
        out DiagnosticIndexOps.IncomingDiagnostic incoming)
    {
        incoming = default!;
        if (!TryResolveDocId(diagnostic.FilePath, runtime, out var docId))
            return false;

        if (!runtime.Registry.ContainsKey(docId))
            return false;

        var meta = runtime.Registry[docId];
        incoming = new DiagnosticIndexOps.IncomingDiagnostic(
            diagnostic.Code,
            diagnostic.Severity,
            diagnostic.Message,
            docId,
            diagnostic.Span,
            ToFSharpList(diagnostic.Tags),
            diagnostic.Language,
            meta.SurfaceVersion);
        return true;
    }

    static string WireSeverity(Severity severity)
    {
        if (severity.IsError)
            return "error";
        if (severity.IsWarning)
            return "warning";
        if (severity.IsInfo)
            return "info";
        if (severity.IsHint)
            return "hint";
        return "info";
    }

    static FSharpList<string> ToFSharpList(IReadOnlyList<string>? tags)
    {
        if (tags is null || tags.Count == 0)
            return ListModule.Empty<string>();

        return ListModule.OfSeq(tags);
    }
}
