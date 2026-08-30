#nullable enable
using System.Text.Json;
using AIGuiders.Platform.Abstractions;

namespace AIGuiders.Platform.MCPlane.Conformance;

public static class McPlaneSpecConformance
{
    public static McPlaneSpecDocument Load(string json) =>
        JsonSerializer.Deserialize<McPlaneSpecDocument>(json, JsonOptions)
        ?? throw new InvalidOperationException("MCPlane spec JSON deserialized to null.");

    public static IReadOnlyList<string> ValidateDocument(McPlaneSpecDocument spec)
    {
        var errors = new List<string>();
        foreach (var vector in spec.Vectors)
        {
            if (!TryValidateVector(spec.Surface, vector, out var error))
                errors.Add($"[{vector.Id}] {error}");
        }

        return errors;
    }

    public static bool TryValidateVector(string surface, McPlaneSpecVector vector, out string error)
    {
        error = "";
        return surface switch
        {
            "mcplane-pulse-default" => TryValidatePulseDefault(vector, out error),
            "mcplane-next-hints" => TryValidateNextHints(vector, out error),
            _ => Fail($"unknown surface \"{surface}\".", out error),
        };
    }

    static bool TryValidatePulseDefault(McPlaneSpecVector vector, out string error)
    {
        error = "";
        if (vector.Outcome is null)
            return Fail("outcome is required.", out error);

        var outcome = ToOutcome(vector.Outcome);
        var maxChars = vector.Expect.PulseMaxChars ?? PulseFormat.DefaultMaxChars;
        var envelope = AgentResponseProjection.FromOutcome(outcome, DetailTier.Pulse, pulseMaxChars: maxChars);
        var expect = vector.Expect;

        if (expect.Tier is not null
            && !string.Equals(expect.Tier, envelope.Tier.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            error = $"tier expected {expect.Tier}, got {envelope.Tier}.";
            return false;
        }

        if (expect.PulseMaxChars is not null && envelope.Pulse is not null)
        {
            var maxAllowed = expect.PulseMaxChars.Value + (expect.PulseEndsWithEllipsis is true ? 1 : 0);
            if (envelope.Pulse.Length > maxAllowed)
            {
                error = $"pulse length {envelope.Pulse.Length} exceeds max {maxAllowed}.";
                return false;
            }
        }

        if (expect.PulseEndsWithEllipsis is true
            && (envelope.Pulse is null || !envelope.Pulse.EndsWith('…')))
        {
            error = "pulse expected to end with ellipsis.";
            return false;
        }

        if (expect.OutcomeIncluded is false && envelope.Outcome is not null)
        {
            error = "outcome should not be included at pulse tier.";
            return false;
        }

        return true;
    }

    static bool TryValidateNextHints(McPlaneSpecVector vector, out string error)
    {
        error = "";
        if (vector.Outcome is null)
            return Fail("outcome is required.", out error);

        var next = (vector.Next ?? [])
            .Select(h => new NextHint(h.Kind, h.CommandId, h.ToolName, h.Label))
            .ToList();

        var envelope = AgentResponseProjection.FromOutcome(ToOutcome(vector.Outcome), DetailTier.Pulse, next);
        var expect = vector.Expect;

        if (expect.NextCount is not null && (envelope.Next?.Count ?? 0) != expect.NextCount)
        {
            error = $"next count expected {expect.NextCount}, got {envelope.Next?.Count ?? 0}.";
            return false;
        }

        if (expect.NextKinds is not null)
        {
            var actualKinds = envelope.Next?.Select(n => n.Kind).ToList() ?? [];
            if (!actualKinds.SequenceEqual(expect.NextKinds, StringComparer.OrdinalIgnoreCase))
            {
                error = $"next kinds expected [{string.Join(", ", expect.NextKinds)}], got [{string.Join(", ", actualKinds)}].";
                return false;
            }
        }

        return true;
    }

    static IntentOutcome ToOutcome(McPlaneSpecOutcome spec) =>
        new(spec.Raw, spec.Verb, spec.Ok, Pulse: spec.Pulse, Reason: spec.Reason);

    static bool Fail(string message, out string error)
    {
        error = message;
        return false;
    }

    static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };
}
