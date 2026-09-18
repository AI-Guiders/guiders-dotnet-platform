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
    public void TryParseToAxes_doc_scan_still_ingests_legacy_fml_at_boundary()
    {
        Assert.True(
            BracketResolveBoundary.TryParseToAxes(
                "[F:Program.cs; M:Foo; L:10]",
                out var axes,
                out var path),
            path);

        Assert.Equal("doc-scan", path);
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

    [Fact]
    public void TryParseNav_parses_kind_nav_wire()
    {
        Assert.True(
            BracketResolveBoundary.TryParseNav(
                "[Kind:Nav; File:README.md; Line:10; Command:open]",
                out var axes,
                out var path),
            path);

        Assert.Equal("kind-nav", path);
        Assert.Equal("README.md", axes.File);
        Assert.Equal(10, axes.Line);
        Assert.Equal("open", axes.Command);
    }

    [Fact]
    public void TryFormatNav_emits_command_only_kind_nav()
    {
        Assert.True(
            BracketResolveBoundary.TryFormatNav(
                new NavResolveAxes(null, Command: "restore"),
                out var wire),
            wire);
        Assert.Equal("[Kind:Nav; Command:restore]", wire);
        Assert.True(BracketResolveBoundary.TryParseNav(wire, out var axes, out var path));
        Assert.Equal("kind-nav", path);
        Assert.Null(axes.File);
        Assert.Equal("restore", axes.Command);
    }

    [Fact]
    public void TryParseNav_rejects_legacy_family_navigation()
    {
        Assert.False(
            BracketResolveBoundary.TryParseNav(
                "[Family:navigation;Command:open;Anchor:[F:README.md;L:10]]",
                out _,
                out _));
    }

    [Fact]
    public void TryFormatNav_member_roundtrip()
    {
        Assert.True(
            BracketResolveBoundary.TryFormatNav(
                new NavResolveAxes("CitizenRouteHost.cs", Line: 50, Command: "open", Member: "RunLand"),
                out var wire),
            wire);
        Assert.Equal(
            "[Kind:Nav; File:CitizenRouteHost.cs; Line:50; Member:RunLand; Command:open]",
            wire);
        Assert.True(BracketResolveBoundary.TryParseNav(wire, out var axes, out var path));
        Assert.Equal("kind-nav", path);
        Assert.Equal("RunLand", axes.Member);
    }
}
