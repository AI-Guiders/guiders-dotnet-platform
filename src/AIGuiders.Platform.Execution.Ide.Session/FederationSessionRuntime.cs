using System.Collections.Concurrent;
using AIGuiders.Platform.Modeling.Ide.Session;
using AIGuiders.Platform.Modeling.Ide.Session.Ports.DotNet;

namespace AIGuiders.Platform.Execution.Ide.Session;

/// <summary>ADR-0062 §5 — result of orchestrator <c>EnsureCompilerServices</c> before LRC dispatch.</summary>
public sealed record FederationCompilerServicesEnsure(
    bool Ok,
    string? Topology,
    string? LanguageId,
    int MaterializedCount,
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
            var cachedValidation = GraphValidation.validate(cached.Session.Graph);
            return new FederationSessionOpenResult(cached, cachedValidation);
        }

        var session = DotNetSlnxGraphPort.loadSession(full);
        var validation = GraphValidation.validate(session.Graph);

        var contents = SessionOrchestrator.loadContentsFromDisk(session.Graph);
        var runtime = SessionOrchestrator.create(session, contents);
        Cache[full] = runtime;

        return new FederationSessionOpenResult(runtime, validation);
    }

    public static FederationCompilerServicesEnsure TryEnsureCompilerServices(string anchorPath, string filePath)
    {
        if (string.IsNullOrWhiteSpace(anchorPath) || string.IsNullOrWhiteSpace(filePath))
            return new FederationCompilerServicesEnsure(false, null, null, 0, "anchor_or_file_missing");

        try
        {
            var fullAnchor = Path.GetFullPath(anchorPath.Trim());
            var opened = Open(anchorPath);
            return ApplyEnsure(opened.Runtime, filePath, fullAnchor);
        }
        catch (Exception ex)
        {
            return new FederationCompilerServicesEnsure(false, null, null, 0, ex.Message);
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
                StoreRuntime(applied.Item.Session.Graph.AnchorPath, applied.Item),
            PatchApplyResult.PatchRejected rejected => new FederationApplyResult(false, runtime, rejected.reasons),
            _ => new FederationApplyResult(false, runtime, ["unknown_patch_apply_result"])
        };
    }

    static FederationCompilerServicesEnsure ApplyEnsure(SessionRuntime runtime, string filePath, string fullAnchor)
    {
        return SessionOrchestrator.ensureCompilerServices(runtime, filePath) switch
        {
            CompilerServicesEnsureResult.Ensured ensured => ToEnsureResult(ensured, fullAnchor),
            CompilerServicesEnsureResult.Failed failed => new FederationCompilerServicesEnsure(
                false,
                null,
                null,
                runtime.Materialized.Entries.Count,
                failed.reason),
            _ => new FederationCompilerServicesEnsure(
                false,
                null,
                null,
                runtime.Materialized.Entries.Count,
                "unknown_ensure_result")
        };
    }

    static FederationCompilerServicesEnsure ToEnsureResult(
        CompilerServicesEnsureResult.Ensured ensured,
        string fullAnchor)
    {
        var mat = ensured.Item1;
        var runtime = ensured.Item2;
        Cache[fullAnchor] = runtime;

        return new FederationCompilerServicesEnsure(
            true,
            mat.TopologyWire,
            mat.LanguageId,
            runtime.Materialized.Entries.Count);
    }

    static FederationApplyResult StoreRuntime(string anchorPath, SessionRuntime runtime)
    {
        if (!string.IsNullOrWhiteSpace(anchorPath))
            Cache[Path.GetFullPath(anchorPath.Trim())] = runtime;

        return new FederationApplyResult(true, runtime, []);
    }
}

public sealed record FederationSessionOpenResult(SessionRuntime Runtime, GraphValidationResult Validation)
{
    public bool IsValid => Validation.IsValid;
}

public sealed record FederationApplyResult(bool Ok, SessionRuntime Runtime, IReadOnlyList<string> Reasons);
