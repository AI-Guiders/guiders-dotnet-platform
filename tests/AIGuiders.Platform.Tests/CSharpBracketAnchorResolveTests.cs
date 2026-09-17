#nullable enable

using AIGuiders.Platform.Execution.LanguageIntelligence.Relations;
using AIGuiders.Platform.Execution.Language.CSharp.Relations;
using Xunit;

namespace AIGuiders.Platform.Tests;

/// <summary>
/// Oracle tests for C# bracket attach resolve — same contract as CDP <c>BracketSyntaxResolve</c> / roslyn-mcp code navigation.
/// </summary>
public sealed class CSharpBracketAnchorResolveTests
{
    [Fact]
    public void TryFindAttachTarget_resolves_member_axis()
    {
        var dir = Path.Combine(Path.GetTempPath(), "guiders-csharp-anchor-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var file = Path.Combine(dir, "Sample.cs");
        File.WriteAllText(
            file,
            """
            namespace Demo;

            public class Sample
            {
                public int GetValue() => 42;
            }
            """);

        var span = RelationWireBoundary.Parse("[F:Sample.cs; M:GetValue]");

        Assert.True(
            CSharpBracketAnchorResolve.TryFindAttachTarget(file, span, out var target, out var detail),
            detail);
        Assert.Equal("member", detail);
        Assert.Contains("GetValue", target.Node.ToString(), StringComparison.Ordinal);

        Directory.Delete(dir, recursive: true);
    }

    [Fact]
    public void TryResolve_line_axis_matches_roslyn_mcp_style_range()
    {
        var dir = Path.Combine(Path.GetTempPath(), "guiders-csharp-anchor-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var file = Path.Combine(dir, "Lines.cs");
        File.WriteAllText(
            file,
            """
            public class Lines
            {
                public void Alpha() { }
                public void Beta() { }
            }
            """);

        var span = RelationWireBoundary.Parse("[F:Lines.cs; L:3]");

        Assert.True(
            CSharpBracketAnchorResolve.TryResolve(file, span, out var range, out var detail),
            detail);
        Assert.Equal(3, range.LineStart);
        Assert.True(range.LineEnd >= range.LineStart);

        Directory.Delete(dir, recursive: true);
    }

    [Fact]
    public void TryResolve_Kind_CodeEdit_wire_via_RelationSpec_path()
    {
        var dir = Path.Combine(Path.GetTempPath(), "guiders-csharp-relspec-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var file = Path.Combine(dir, "Sample.cs");
        File.WriteAllText(
            file,
            """
            namespace Demo;

            public class Sample
            {
                public int GetValue() => 42;
            }
            """);

        var spec = RelationSpecWireBoundary.TryParseKindSpec("[Kind:CodeEdit; File:Sample.cs; Member:GetValue]");
        Assert.NotNull(spec);

        Assert.True(
            CSharpBracketAnchorResolve.TryResolve(file, spec!, out var range, out var detail),
            detail);
        Assert.Equal("member", detail);
        Assert.True(range.LineStart >= 1);

        Directory.Delete(dir, recursive: true);
    }
}
