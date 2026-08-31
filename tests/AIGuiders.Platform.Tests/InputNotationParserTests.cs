using AIGuiders.Platform.IntermediateRepresentation.Melody;
using AIGuiders.Platform.IntermediateRepresentation.Keyboard;
using AIGuiders.Platform.Notations.Keyboard;
using AIGuiders.Platform.CommandPlane.Melody;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class InputNotationParserTests
{
    [Fact]
    public void Vim_empty_string_yields_success_zero_steps()
    {
        var r = VimChordNotationParser.Parse("");
        Assert.True(r.IsSuccess);
        Assert.Empty(r.Steps);
    }

    [Fact]
    public void Vim_cascade_chord_palette_steps()
    {
        var r = VimChordNotationParser.Parse("<C-k> s p");
        Assert.True(r.IsSuccess);
        Assert.Collection(
            r.Steps,
            s => AssertVimChord(s, ["C-"], "k"),
            s => AssertVimPlain(s, "s"),
            s => AssertVimPlain(s, "p"));
    }

    [Fact]
    public void Vim_and_KeyGesture_normalize_to_same_sequence()
    {
        Assert.True(VimChordNotationParser.TryParseToNormalized("<C-k> s p", out var vim, out _));
        Assert.True(KeyGestureChordSyntax.TryParseToNormalized("Ctrl+K s p", out var kg, out _));
        Assert.NotNull(vim);
        Assert.NotNull(kg);
        Assert.Equal(3, vim!.Steps.Count);
        Assert.Equal(vim.Steps.Count, kg!.Steps.Count);
        AssertNormalizedChord(vim.Steps[0], ChordModifierKeys.Control, "K");
        AssertNormalizedChord(kg.Steps[0], ChordModifierKeys.Control, "K");
        AssertNormalizedPlain(vim.Steps[1], "S");
        AssertNormalizedPlain(kg.Steps[1], "S");
    }

    [Fact]
    public void KeyGesture_ctrl_shift_p_and_unicode_command_key()
    {
        Assert.True(KeyGestureChordSyntax.TryParseToNormalized("Ctrl + Shift + P", out var a, out _));
        Assert.NotNull(a);
        Assert.Single(a!.Steps);
        var ch = Assert.IsType<NormalizedChordStep>(a.Steps[0]);
        Assert.Equal(ChordModifierKeys.Control | ChordModifierKeys.Shift, ch.Modifiers);
        Assert.Equal("P", ch.KeySymbol);

        Assert.True(KeyGestureChordSyntax.TryParseToNormalized("\u2318K", out var b, out _));
        var ch2 = Assert.IsType<NormalizedChordStep>(b!.Steps[0]);
        Assert.Equal(ChordModifierKeys.Meta, ch2.Modifiers);
        Assert.Equal("K", ch2.KeySymbol);
    }

    [Fact]
    public void Facade_dispatches_by_surface()
    {
        Assert.True(KeyboardNotationParser.TryParseToSequence(
            "Ctrl+K",
            KeyboardNotationSurface.KeyGestureConfig,
            out var kg,
            out _));
        Assert.Single(kg!.Steps);

        Assert.True(KeyboardNotationParser.TryParseToSequence(
            "<C-k>",
            KeyboardNotationSurface.VimDocument,
            out var vim,
            out _));
        Assert.Single(vim!.Steps);
    }

    [Fact]
    public void Melody_note_notation_parses_plain_token()
    {
        Assert.True(MelodyNoteNotation.TryParseStep("b", out var step, out _));
        var plain = Assert.IsType<NormalizedPlainKeyStep>(step);
        Assert.Equal("B", plain.KeySymbol);
    }

    [Fact]
    public void Melody_step_notation_normalizes_slug_line()
    {
        var descriptor = MelodyDescriptor.FromSlug("git.status", "gs");
        Assert.True(MelodyStepNotation.TryNormalizeLine(descriptor, out var sequence, out var error), error);
        Assert.Equal(2, sequence!.Steps.Count);
        AssertNormalizedPlain(sequence.Steps[0], "G");
        AssertNormalizedPlain(sequence.Steps[1], "S");
    }

    [Fact]
    public void Melody_by_chord_step_normalizes_single_gesture()
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

        Assert.True(MelodyStepNotation.TryNormalizeLine(descriptor, out var sequence, out var error), error);
        Assert.Equal(2, sequence!.Steps.Count);
        Assert.All(sequence.Steps, s => Assert.IsType<NormalizedChordStep>(s));
    }

    static void AssertVimPlain(VimNotationStep step, string token)
    {
        var p = Assert.IsType<VimNotationPlainStep>(step);
        Assert.Equal(token, p.Token);
    }

    static void AssertVimChord(VimNotationStep step, string[] mods, string key)
    {
        var c = Assert.IsType<VimNotationChordStep>(step);
        Assert.Equal(mods, c.ModifierPrefixes);
        Assert.Equal(key, c.Key);
    }

    static void AssertNormalizedChord(NormalizedSequenceStep step, ChordModifierKeys mods, string key)
    {
        var c = Assert.IsType<NormalizedChordStep>(step);
        Assert.Equal(mods, c.Modifiers);
        Assert.Equal(key, c.KeySymbol);
    }

    static void AssertNormalizedPlain(NormalizedSequenceStep step, string key)
    {
        var p = Assert.IsType<NormalizedPlainKeyStep>(step);
        Assert.Equal(key, p.KeySymbol);
    }
}
