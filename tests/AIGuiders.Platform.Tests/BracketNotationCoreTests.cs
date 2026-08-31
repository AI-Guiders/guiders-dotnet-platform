#nullable enable
using AIGuiders.Platform.Notations;
using AIGuiders.Platform.IntermediateRepresentation.Bracket;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class BracketNotationCoreTests
{
    [Fact]
    public void NotationKvPair_splits_on_first_sign_only()
    {
        Assert.True(NotationKvPair.TrySplitFirst("S:for:2", ':', out var kv, out _));
        Assert.Equal("S", kv.Key);
        Assert.Equal("for:2", kv.Value);

        Assert.True(NotationKvPair.TrySplitFirst("doc=README.md", '=', out var console, out _));
        Assert.Equal("doc", console.Key);
        Assert.Equal("README.md", console.Value);
    }

    [Fact]
    public void BracketReader_parses_cdp_square_kv()
    {
        Assert.True(
            BracketReader.Default.TryRead(
                "[F:Program.cs;M:Foo]",
                BracketProfiles.CdpSquareKeyValue,
                out var wire,
                out var error),
            error);

        Assert.NotNull(wire);
        Assert.Equal(2, wire!.Axes.Count);
        Assert.Equal("F", wire.Axes[0].Key);
        Assert.Equal("Program.cs", wire.Axes[0].Value);
        Assert.Equal(BracketAxisValueClasses.CommandPath, wire.Axes[0].ValueWireClass);
    }

    [Fact]
    public void BracketReader_parses_nested_anchor_at_depth()
    {
        Assert.True(
            BracketReader.Default.TryRead(
                "[Family:navigation;Command:open;Anchor:[F:README.md;L:10]]",
                BracketProfiles.CdpSquareKeyValue,
                out var wire,
                out var error),
            error);

        Assert.NotNull(wire);
        var anchor = wire!.Axes[2];
        Assert.Equal("Anchor", anchor.Key);
        Assert.NotNull(anchor.Nested);
        Assert.Equal("README.md", anchor.Nested!.Axes[0].Value);
    }

    [Fact]
    public void BracketReader_accepts_bare_inner_without_terminals()
    {
        Assert.True(
            BracketReader.Default.TryRead(
                "F:Program.cs;M:Foo",
                BracketProfiles.CdpSquareKeyValue,
                out var wire,
                out var error),
            error);
        Assert.Equal(2, wire!.Axes.Count);
    }

    [Fact]
    public void BracketReader_angle_opaque_profile()
    {
        Assert.True(
            BracketReader.Default.TryRead("<C-k>", BracketProfiles.AngleOpaque, out var wire, out var error),
            error);
        Assert.Equal("C-k", wire!.Axes[0].Value);
    }
}
