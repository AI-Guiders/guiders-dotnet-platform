using AIGuiders.Platform.IntermediateRepresentation.Melody;
using AIGuiders.Platform.Modeling.Notations.Keyboard;
#nullable enable
using AIGuiders.Platform.Notations.Keyboard;

namespace AIGuiders.Platform.Execution.CommandPlane.Melody;

/// <summary>Bridge <see cref="MelodyStep"/> wires to normalized input steps.</summary>
public static class MelodyStepNotation
{
    public static bool TryNormalizeStep(MelodyStep step, out NormalizedSequenceStep? normalized, out string error)
    {
        normalized = null;
        error = "";

        if (step.Articulation == MelodyArticulation.ByNote)
            return MelodyNoteNotation.TryParseStep(step.Wire, out normalized, out error);

        if (!KeyGestureChordSyntax.TryParseToNormalized(step.Wire, out var sequence, out error))
            return false;

        if (sequence!.Steps.Count != 1)
        {
            error = "ByChord melody step must be a single chord gesture wire.";
            return false;
        }

        normalized = sequence.Steps[0];
        return true;
    }

    public static bool TryNormalizeLine(
        MelodyDescriptor descriptor,
        out NormalizedKeySequence? sequence,
        out string error)
    {
        sequence = null;
        error = "";

        if (!MelodyLinePolicy.TryNormalize(descriptor, out var normalized, out var policyErrors))
        {
            error = string.Join("; ", policyErrors);
            return false;
        }

        var steps = new List<NormalizedSequenceStep>(normalized.Steps.Count);
        foreach (var step in normalized.Steps)
        {
            if (!TryNormalizeStep(step, out var normalizedStep, out error))
                return false;

            steps.Add(normalizedStep!);
        }

        sequence = new NormalizedKeySequence(steps);
        error = "";
        return true;
    }
}
