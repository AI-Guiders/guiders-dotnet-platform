using AIGuiders.Platform.Abstractions;

namespace AIGuiders.Platform.MCPlane;

public static class AgentResponseProjection
{
    public static AgentResponseEnvelope FromOutcome(
        IntentOutcome outcome,
        DetailTier tier = DetailTier.Pulse,
        IReadOnlyList<NextHint>? next = null,
        int pulseMaxChars = PulseFormat.DefaultMaxChars)
    {
        var pulse = tier switch
        {
            DetailTier.Pulse => PulseFormat.Truncate(outcome.Pulse ?? outcome.Raw, pulseMaxChars),
            DetailTier.Slim => outcome.Pulse ?? outcome.Raw,
            DetailTier.Full => outcome.Raw,
            DetailTier.Wide => outcome.Raw,
            _ => PulseFormat.Truncate(outcome.Pulse ?? outcome.Raw, pulseMaxChars),
        };

        return new AgentResponseEnvelope(
            outcome.Ok,
            tier,
            pulse,
            outcome.Reason,
            next,
            tier is DetailTier.Full or DetailTier.Wide ? outcome : null);
    }
}
