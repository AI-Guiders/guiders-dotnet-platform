#nullable enable

namespace AIGuiders.Platform.Execution.Cockpit.Composition;

/// <summary>Generic surface compositor contract (CIDE ADR 0036): scene + payload + decision → surface snapshot.</summary>
public interface ISurfaceCompositor<TScene, TPayload, TDecision, TResult>
{
    TResult Compose(TScene scene, TPayload payload, in TDecision decision);
}
