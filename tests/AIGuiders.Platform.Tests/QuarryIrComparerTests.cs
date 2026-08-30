#nullable enable
using AIGuiders.Platform.Notations.Keyboard;
using AIGuiders.Platform.Notations.Keyboard.Quarry;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class QuarryIrComparerTests
{
    [Fact]
    public void Sequences_equal_when_steps_match()
    {
        var left = new NormalizedKeySequence(
        [
            new NormalizedChordStep(ChordModifierKeys.Control, "X"),
            new NormalizedPlainKeyStep("S"),
        ]);
        var right = new NormalizedKeySequence(
        [
            new NormalizedChordStep(ChordModifierKeys.Control, "X"),
            new NormalizedPlainKeyStep("S"),
        ]);

        Assert.True(QuarryIrComparer.SequencesEqual(left, right, out _));
    }

    [Fact]
    public void Oracle_json_maps_to_normalized_sequence()
    {
        var steps = new[]
        {
            new QuarryOracleStepJson("chord", "Control|Shift", "P"),
            new QuarryOracleStepJson("plain", null, "S"),
        };

        var sequence = QuarryOracleIrMapper.ToNormalized(steps);
        Assert.Equal(2, sequence.Steps.Count);
        Assert.IsType<NormalizedChordStep>(sequence.Steps[0]);
        Assert.Equal(ChordModifierKeys.Control | ChordModifierKeys.Shift, ((NormalizedChordStep)sequence.Steps[0]).Modifiers);
    }
}
