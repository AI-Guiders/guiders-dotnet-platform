#nullable enable
using AIGuiders.Platform.CommandPlane.Melody;
using AIGuiders.Platform.Notations.Argument;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class MelodyLinePolicyTests
{
    [Fact]
    public void FromSlug_infers_by_note_steps()
    {
        var descriptor = MelodyDescriptor.FromSlug("git.status", "gs", "Git Status");
        Assert.True(MelodyLinePolicy.TryNormalize(descriptor, out var normalized, out var errors), string.Join("; ", errors));

        Assert.Equal(MelodyLineProfile.PureByNote, normalized.Profile);
        Assert.Equal(2, normalized.Steps.Count);
        Assert.All(normalized.Steps, s => Assert.Equal(MelodyArticulation.ByNote, s.Articulation));
        Assert.Equal(["g", "s"], normalized.Steps.Select(s => s.Wire).ToArray());
    }

    [Fact]
    public void PureByChord_requires_explicit_steps()
    {
        var descriptor = new MelodyDescriptor
        {
            CommandId = "redo.twice",
            Slug = "redo2",
            Profile = MelodyLineProfile.PureByChord,
        };

        var errors = MelodyLinePolicy.Validate(descriptor);

        Assert.Contains(errors, e => e.Contains("Explicit steps", StringComparison.Ordinal));
    }

    [Fact]
    public void PureByChord_accepts_chord_steps()
    {
        var descriptor = new MelodyDescriptor
        {
            CommandId = "redo.twice",
            Slug = "rr",
            Profile = MelodyLineProfile.PureByChord,
            Steps =
            [
                new MelodyStep { Articulation = MelodyArticulation.ByChord, Wire = "Ctrl+R" },
                new MelodyStep { Articulation = MelodyArticulation.ByChord, Wire = "Ctrl+R" },
            ],
        };

        Assert.True(MelodyLinePolicy.TryNormalize(descriptor, out _, out var errors), string.Join("; ", errors));
    }

    [Fact]
    public void Mixed_requires_two_articulations()
    {
        var descriptor = new MelodyDescriptor
        {
            CommandId = "sample.mixed",
            Slug = "bx",
            Profile = MelodyLineProfile.Mixed,
            Steps =
            [
                new MelodyStep { Articulation = MelodyArticulation.ByNote, Wire = "b" },
                new MelodyStep { Articulation = MelodyArticulation.ByChord, Wire = "Ctrl+Shift+P" },
            ],
        };

        Assert.True(MelodyLinePolicy.TryNormalize(descriptor, out _, out var errors), string.Join("; ", errors));
    }

    [Fact]
    public void Mixed_rejects_single_articulation()
    {
        var descriptor = new MelodyDescriptor
        {
            CommandId = "sample.bad",
            Slug = "bs",
            Profile = MelodyLineProfile.Mixed,
            Steps =
            [
                new MelodyStep { Articulation = MelodyArticulation.ByNote, Wire = "b" },
                new MelodyStep { Articulation = MelodyArticulation.ByNote, Wire = "s" },
            ],
        };

        var errors = MelodyLinePolicy.Validate(descriptor);

        Assert.Contains(errors, e => e.Contains("Mixed profile", StringComparison.Ordinal));
    }

    [Fact]
    public void PureByNote_rejects_chord_step()
    {
        var descriptor = new MelodyDescriptor
        {
            CommandId = "sample.bad",
            Slug = "br",
            Profile = MelodyLineProfile.PureByNote,
            Steps =
            [
                new MelodyStep { Articulation = MelodyArticulation.ByNote, Wire = "b" },
                new MelodyStep { Articulation = MelodyArticulation.ByChord, Wire = "Ctrl+R" },
            ],
        };

        var errors = MelodyLinePolicy.Validate(descriptor);

        Assert.Contains(errors, e => e.Contains("PureByNote", StringComparison.Ordinal));
    }

    [Fact]
    public void ToLine_projects_descriptor_fields()
    {
        var descriptor = new MelodyDescriptor
        {
            CommandId = "git.status",
            Slug = "gs",
            Help = "Git Status",
            ArgumentNotation = new ArgumentNotationProfile("line_range"),
        };

        var line = descriptor.ToLine();

        Assert.Equal("gs", line.Slug);
        Assert.Equal("Git Status", line.Help);
        Assert.Equal("line_range", line.ArgumentNotation!.ReaderId);
    }
}
