using AIGuiders.Platform.Abstractions;

namespace AIGuiders.Platform.IntermediateRepresentation.Agent;

public sealed record AgentResponseEnvelope(
    bool Ok,
    DetailTier Tier,
    string? Pulse,
    string? Reason,
    IReadOnlyList<NextHint>? Next,
    IntentOutcome? Outcome = null);
