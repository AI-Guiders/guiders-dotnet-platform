#nullable enable
using System.Reflection;
using AIGuiders.Platform.InputNotation;
using AIGuiders.Platform.InputNotation.Quarry;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class QuarryNotationConformanceTests
{
    [Fact]
    public void Neovim_spec_v1_vectors_conform()
    {
        var spec = LoadSpec("AIGuiders.Platform.Tests.Fixtures.Quarry.neovim-kbd-v1.spec.json");
        Assert.Equal("neovim-kbd", spec.Surface);
        Assert.Empty(QuarrySpecConformance.ValidateDocument(NeovimNotationReader.Instance, spec));
    }

    [Fact]
    public void Emacs_spec_v1_vectors_conform()
    {
        var spec = LoadSpec("AIGuiders.Platform.Tests.Fixtures.Quarry.emacs-kbd-v1.spec.json");
        Assert.Equal("emacs-kbd", spec.Surface);
        Assert.Empty(QuarrySpecConformance.ValidateDocument(EmacsNotationReader.Instance, spec));
    }

    [Fact]
    public void Neovim_and_Vim_cascade_line_share_IR_for_cide_subset()
    {
        Assert.True(NeovimKeyNotationParser.TryParseToNormalized("<C-k> s p", out var neovim, out _));
        Assert.True(VimChordNotationParser.TryParseToNormalized("<C-k> s p", out var vim, out _));
        Assert.NotNull(neovim);
        Assert.NotNull(vim);
        Assert.Equal(neovim!.Steps.Count, vim!.Steps.Count);
        for (var i = 0; i < neovim.Steps.Count; i++)
            Assert.Equal(neovim.Steps[i], vim.Steps[i]);
    }

    [Fact]
    public void Emacs_and_Neovim_control_x_share_IR()
    {
        Assert.True(EmacsKbdNotationParser.TryParseToNormalized("C-x", out var emacs, out _));
        Assert.True(NeovimKeyNotationParser.TryParseToNormalized("<C-x>", out var neovim, out _));
        AssertNormalizedChord(emacs!.Steps[0], ChordModifierKeys.Control, "X");
        AssertNormalizedChord(neovim!.Steps[0], ChordModifierKeys.Control, "X");
    }

    static QuarrySpecDocument LoadSpec(string resourceName)
    {
        var asm = Assembly.GetExecutingAssembly();
        using var stream = asm.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Missing embedded resource: {resourceName}");
        using var reader = new StreamReader(stream);
        return QuarrySpecLoader.Load(reader.ReadToEnd());
    }

    static void AssertNormalizedChord(NormalizedSequenceStep step, ChordModifierKeys mods, string key)
    {
        var c = Assert.IsType<NormalizedChordStep>(step);
        Assert.Equal(mods, c.Modifiers);
        Assert.Equal(key, c.KeySymbol);
    }
}
