using AIGuiders.Platform.Modeling.Ide.Session;
using AIGuiders.Platform.Modeling.Ide.Session.Ports.DotNet;

namespace AIGuiders.Platform.Execution.Ide.Session;

/// <summary>Federation session runtime: graph SSOT + contents + M + Λ orchestration.</summary>
public static class FederationSessionRuntime
{
    public static FederationSessionOpenResult Open(string anchorPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(anchorPath);

        var session = DotNetSlnxGraphPort.loadSession(anchorPath);
        var validation = GraphValidation.validate(session.Graph);

        var contents = SessionOrchestrator.loadContentsFromDisk(session.Graph);
        var runtime = SessionOrchestrator.create(session, contents);

        return new FederationSessionOpenResult(runtime, validation);
    }

    public static FederationApplyResult TryApplyPatch(SessionRuntime runtime, SessionPatch patch, GitPin? gitPin = null)
    {
        ArgumentNullException.ThrowIfNull(runtime);
        ArgumentNullException.ThrowIfNull(patch);

        var pin = gitPin ?? new GitPin(null);

        return SessionOrchestrator.applyPatch(runtime, patch, pin) switch
        {
            PatchApplyResult.PatchApplied applied => new FederationApplyResult(true, applied.Item, []),
            PatchApplyResult.PatchRejected rejected => new FederationApplyResult(false, runtime, rejected.reasons),
            _ => new FederationApplyResult(false, runtime, ["unknown_patch_apply_result"])
        };
    }
}

public sealed record FederationSessionOpenResult(SessionRuntime Runtime, GraphValidationResult Validation)
{
    public bool IsValid => Validation.IsValid;
}

public sealed record FederationApplyResult(bool Ok, SessionRuntime Runtime, IReadOnlyList<string> Reasons);
