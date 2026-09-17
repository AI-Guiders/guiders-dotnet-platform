using System.Collections.Concurrent;
using AIGuiders.Platform.Modeling.Core.Identity;
using AIGuiders.Platform.Modeling.Ide.Session;
using AIGuiders.Platform.Modeling.Ide.Session.Ports.DotNet;
using AIGuiders.Platform.Modeling.Language;
using AIGuiders.Platform.Modeling.LanguageIntelligence.Relations;
using AIGuiders.Platform.Execution.Language.Adapters.Fcs;
using Microsoft.FSharp.Collections;
namespace AIGuiders.Platform.Execution.Ide.Session;

/// <summary>ADR-0062 §5 — result of orchestrator <c>EnsureCompilerServices</c> before LRC dispatch.</summary>
public sealed record FederationCompilerServicesEnsure(
    bool Ok,
    string? Topology,
    string? LanguageId,
    int MaterializedCount,
    AIGuiders.Platform.Modeling.Ide.Session.WorkspaceView? WorkspaceView = null,
    string? Reason = null);

/// <summary>Federation session runtime: graph SSOT + contents + M + Λ orchestration.</summary>
public static class FederationSessionRuntime
{
    static readonly ConcurrentDictionary<string, SessionRuntime> Cache = new(StringComparer.OrdinalIgnoreCase);

    public static FederationSessionOpenResult Open(string anchorPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(anchorPath);

        var full = Path.GetFullPath(anchorPath.Trim());
        if (Cache.TryGetValue(full, out var cached))
        {
            var cachedValidation = GraphValidation.validate(cached.Session.Graph, cached.Registry);
            return new FederationSessionOpenResult(cached, cachedValidation);
        }

        var session = DotNetSlnxGraphSources.LoadSession(full);
        var ownership = DotNetSlnxGraphSources.LoadDocumentOwnership(full);
        var contents = SessionContentsLoader.LoadFromDisk(ownership);
        var runtime = SessionOrchestrator.create(session, MapModule.ToSeq(contents), ownership);
        var validation = GraphValidation.validate(runtime.Session.Graph, runtime.Registry);
        Cache[full] = runtime;

        return new FederationSessionOpenResult(runtime, validation);
    }

    /// <summary>Replace session diagnostic index from LRC refresh (plan §2.4.2).</summary>
    public static DiagnosticIndexIngest.IngestResult? TryRefreshDiagnosticIndex(
        string anchorPath,
        DiagnosticsResult diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);
        if (string.IsNullOrWhiteSpace(anchorPath))
            return null;

        var full = Path.GetFullPath(anchorPath.Trim());
        if (!Cache.TryGetValue(full, out var runtime))
        {
            try
            {
                runtime = Open(anchorPath).Runtime;
            }
            catch (Exception)
            {
                return null;
            }
        }

        var result = DiagnosticIndexIngest.RefreshLrc(diagnostics.Diagnostics ?? [], runtime);
        Cache[full] = result.Runtime;
        return result;
    }

    public static FederationCompilerServicesEnsure TryEnsureCompilerServices(string anchorPath, string filePath)
    {
        if (string.IsNullOrWhiteSpace(anchorPath) || string.IsNullOrWhiteSpace(filePath))
            return new FederationCompilerServicesEnsure(false, null, null, 0, Reason: "anchor_or_file_missing");

        try
        {
            var fullAnchor = Path.GetFullPath(anchorPath.Trim());
            var opened = Open(anchorPath);
            return ApplyEnsure(opened.Runtime, filePath, fullAnchor);
        }
        catch (Exception ex)
        {
            return new FederationCompilerServicesEnsure(false, null, null, 0, Reason: ex.Message);
        }
    }

    public static FederationApplyResult TryApplyPatch(SessionRuntime runtime, SessionPatch patch, GitPin? gitPin = null)
    {
        ArgumentNullException.ThrowIfNull(runtime);
        ArgumentNullException.ThrowIfNull(patch);

        var pin = gitPin ?? new GitPin(null);

        return SessionOrchestrator.applyPatch(runtime, patch, pin) switch
        {
            PatchApplyResult.PatchApplied applied =>
                StoreRuntime(applied.Item.Session.Graph.Anchor.Value, applied.Item),
            PatchApplyResult.PatchRejected rejected => new FederationApplyResult(false, runtime, rejected.reasons),
            _ => new FederationApplyResult(false, runtime, ["unknown_patch_apply_result"])
        };
    }

    static FederationCompilerServicesEnsure ApplyEnsure(SessionRuntime runtime, string filePath, string fullAnchor)
    {
        return DesignTimeCompilerServicesPort.materialize(runtime, filePath) switch
        {
            CompilerServicesEnsureResult.Ensured ensured => ToEnsureResult(ensured.Item1, ensured.Item2, fullAnchor),
            CompilerServicesEnsureResult.Failed failed => new FederationCompilerServicesEnsure(
                false,
                null,
                null,
                runtime.Materialized.Entries.Count,
                Reason: failed.reason),
            _ => new FederationCompilerServicesEnsure(
                false,
                null,
                null,
                runtime.Materialized.Entries.Count,
                Reason: "unknown_ensure_result")
        };
    }

    static FederationCompilerServicesEnsure ToEnsureResult(
        CompilerServicesMaterialization mat,
        SessionRuntime runtime,
        string fullAnchor)
    {
        Cache[fullAnchor] = runtime;

        var view = mat.WorkspaceView;
        if (view is not null
            && string.Equals(mat.LanguageId, "fsharp", StringComparison.OrdinalIgnoreCase))
        {
            view = FcsExecutionHost.Materialize(view);
        }

        return new FederationCompilerServicesEnsure(
            true,
            mat.TopologyWire,
            mat.LanguageId,
            runtime.Materialized.Entries.Count,
            view);
    }

    static FederationApplyResult StoreRuntime(string anchorPath, SessionRuntime runtime)
    {
        if (!string.IsNullOrWhiteSpace(anchorPath))
        {
            var full = Path.GetFullPath(anchorPath.Trim());
            Cache[full] = runtime;
            FcsExecutionHost.Invalidate(full);
        }

        return new FederationApplyResult(true, runtime, []);
    }
}

public sealed record FederationSessionOpenResult(SessionRuntime Runtime, GraphValidationResult Validation)
{
    public bool IsValid => Validation.IsValid;
}

public sealed record FederationApplyResult(bool Ok, SessionRuntime Runtime, IReadOnlyList<string> Reasons);
