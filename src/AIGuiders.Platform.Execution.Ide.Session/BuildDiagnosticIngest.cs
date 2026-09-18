#nullable enable

using AIGuiders.Platform.Modeling.Build;
using AIGuiders.Platform.Modeling.Ide.Session;
using AIGuiders.Platform.Modeling.LanguageIntelligence.Relations;
using Microsoft.FSharp.Collections;

namespace AIGuiders.Platform.Execution.Ide.Session;

/// <summary>
/// Execution ingest: build toolchain diagnostics → session DiagnosticIndex + RelationSpec.Diag witnesses.
/// </summary>
public static class BuildDiagnosticIngest
{
    public sealed record BuildDiagnosticWire(
        string File,
        int Line,
        int Column,
        string Code,
        string Message);

    public sealed record IngestResult(SessionRuntime Runtime, IReadOnlyList<BuildDiagnostic> Diagnostics, int SkippedUnregistered);

    public static IngestResult Ingest(IEnumerable<BuildDiagnosticWire> diagnostics, SessionRuntime runtime)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);
        ArgumentNullException.ThrowIfNull(runtime);

        var raw = diagnostics
            .Select(d => new RawDiagnostic(d.File, d.Line, d.Column, d.Code, d.Message))
            .ToArray();

        var result = BuildDiagnosticOps.ingest(raw, runtime);
        return new IngestResult(result.Runtime, result.Diagnostics, result.SkippedUnregistered);
    }
}
