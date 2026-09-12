using AIGuiders.Platform.Modeling.Core;
using GdlAgent = AIGuiders.Platform.Modeling.Gdl.Agent;

namespace AIGuiders.Platform.IntermediateRepresentation.Agent;

/// <summary>
/// GUIDERS-FSHARP-ADR-0003 §4.3 cutover: wire envelope SSOT via <see cref="GdlAgent.AgentResponseEnvelope"/>;
/// <see cref="Outcome"/> stays execution-side (Modeling.Core IntentOutcome).
/// </summary>
public sealed record AgentResponseEnvelope(
    bool Ok,
    DetailTier Tier,
    string? Pulse,
    string? Reason,
    IReadOnlyList<NextHint>? Next,
    IntentOutcome? Outcome = null)
{
    public GdlAgent.AgentResponseEnvelope ToModel() => new(
        Ok,
        Tier,
        Pulse is null ? Microsoft.FSharp.Core.FSharpOption<string>.None : Microsoft.FSharp.Core.FSharpOption<string>.Some(Pulse),
        Reason is null ? Microsoft.FSharp.Core.FSharpOption<string>.None : Microsoft.FSharp.Core.FSharpOption<string>.Some(Reason),
        Next is null ? Microsoft.FSharp.Core.FSharpOption<IReadOnlyList<NextHint>>.None : Microsoft.FSharp.Core.FSharpOption<IReadOnlyList<NextHint>>.Some(Next));

    public static AgentResponseEnvelope FromModel(GdlAgent.AgentResponseEnvelope model, IntentOutcome? outcome = null) =>
        new(
            model.Ok,
            model.Tier,
            Microsoft.FSharp.Core.FSharpOption<string>.get_IsSome(model.Pulse) ? model.Pulse.Value : null,
            Microsoft.FSharp.Core.FSharpOption<string>.get_IsSome(model.Reason) ? model.Reason.Value : null,
            Microsoft.FSharp.Core.FSharpOption<IReadOnlyList<NextHint>>.get_IsSome(model.Next) ? model.Next.Value : null,
            outcome);
}
