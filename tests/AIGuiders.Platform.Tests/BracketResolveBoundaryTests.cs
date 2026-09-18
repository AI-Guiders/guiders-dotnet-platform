#nullable enable

using AIGuiders.Platform.Execution.LanguageIntelligence.Relations;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class BracketResolveBoundaryTests
{
    [Fact]
    public void TryParseToAxes_prefers_kind_spec_over_legacy_shape()
    {
        Assert.True(
            BracketResolveBoundary.TryParseToAxes(
                "[Kind:CodeEdit; File:Program.cs; Member:Foo; Line:10]",
                out var axes,
                out var path),
            path);

        Assert.Equal("kind-spec", path);
        Assert.Equal("Program.cs", axes.File);
        Assert.Equal("Foo", axes.MemberKey);
        Assert.Equal(10, axes.LineStart);
    }

    [Fact]
    public void TryParseToAxes_falls_back_to_legacy_wire()
    {
        Assert.True(
            BracketResolveBoundary.TryParseToAxes(
                "[F:Program.cs; M:Foo; L:10]",
                out var axes,
                out var path),
            path);

        Assert.Equal("legacy-wire", path);
        Assert.Equal("Program.cs", axes.File);
        Assert.Equal("Foo", axes.MemberKey);
        Assert.Equal(10, axes.LineStart);
    }

    [Fact]
    public void TryFormatCodeEdit_emits_kind_spec_roundtrip()
    {
        Assert.True(
            BracketResolveBoundary.TryFormatCodeEdit(
                new CodeEditResolveAxes("Program.cs", null, 10, 10),
                out var wire),
            wire);

        Assert.Equal("[Kind:CodeEdit; File:Program.cs; Line:10]", wire);
        Assert.True(BracketResolveBoundary.TryParseToAxes(wire, out var axes, out var path));
        Assert.Equal("kind-spec", path);
        Assert.Equal("Program.cs", axes.File);
        Assert.Null(axes.MemberKey);
        Assert.Equal(10, axes.LineStart);
    }
}
