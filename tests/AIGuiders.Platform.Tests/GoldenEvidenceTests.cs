#nullable enable

using AIGuiders.Platform.Execution.Documentation.Correspondence;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class GoldenEvidenceTests
{
    static string GuidersFSharpRoot()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            var sibling = Path.GetFullPath(Path.Combine(dir.FullName, "..", "guiders-fsharp"));
            if (File.Exists(Path.Combine(sibling, "AIGuiders.Platform.Modeling.slnx")))
                return sibling;

            if (File.Exists(Path.Combine(dir.FullName, "AIGuiders.Platform.Modeling.slnx")))
                return dir.FullName;
        }

        throw new InvalidOperationException("Could not locate guiders-fsharp root for GoldenEvidence tests.");
    }

    [Theory]
    [InlineData("GS-OB1")]
    [InlineData("GS-OB2")]
    [InlineData("GS-OB3")]
    public void Locate_finds_open_build_golden_stubs_in_guiders_fsharp(string goldenId)
    {
        var root = GuidersFSharpRoot();
        Assert.True(GoldenEvidence.TryLocate(root, goldenId, out var found));
        Assert.Contains("OpenBuildGoldenTests", found.Path, StringComparison.OrdinalIgnoreCase);
        Assert.False(string.IsNullOrWhiteSpace(found.Hint));
    }

    [Fact]
    public void Locate_returns_false_for_unknown_golden_id()
    {
        var root = GuidersFSharpRoot();
        Assert.False(GoldenEvidence.TryLocate(root, "GS-OB999", out _));
    }

    [Fact]
    public void Exists_is_false_for_blank_golden_id()
    {
        Assert.False(GoldenEvidence.Exists(GuidersFSharpRoot(), ""));
    }
}
