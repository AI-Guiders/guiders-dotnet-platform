using AIGuiders.Platform.Execution.Documentation.Correspondence;
using AIGuiders.Platform.Modeling.Paths;
using Xunit;

namespace AIGuiders.Platform.Tests.Paths;

public sealed class LogicalPathTests
{
    [Theory]
    [InlineData(@"src\Foo\Bar.cs", "src/Foo/Bar.cs")]
    [InlineData("/docs/adr/0047-x.md", "docs/adr/0047-x.md")]
    [InlineData("  ./src//x.cs  ", "src/x.cs")]
    public void Normalize_unifies_separators(string raw, string expected) =>
        Assert.Equal(expected, LogicalPath.Normalize(raw));

    [Fact]
    public void PathBoundary_roundtrip_under_workspace()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "guiders-paths-" + Guid.NewGuid().ToString("N")));
        Directory.CreateDirectory(root);
        var nested = Path.Combine(root, "src", "Widget.cs");
        Directory.CreateDirectory(Path.GetDirectoryName(nested)!);
        File.WriteAllText(nested, "// test");

        try
        {
            var logical = PathBoundary.ToLogical(root, nested);
            Assert.NotNull(logical);
            Assert.Equal("src/Widget.cs", logical.Value.Value);

            var physical = PathBoundary.ToPhysical(root, logical.Value);
            Assert.NotNull(physical);
            Assert.True(File.Exists(physical));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void CorrespondencePaths_TryRel_matches_path_boundary()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "guiders-corr-" + Guid.NewGuid().ToString("N")));
        Directory.CreateDirectory(root);
        var file = Path.Combine(root, "docs", "readme.md");
        Directory.CreateDirectory(Path.GetDirectoryName(file)!);
        File.WriteAllText(file, "# x");

        try
        {
            var rel = CorrespondencePaths.TryRel(root, file);
            Assert.Equal("docs/readme.md", rel);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void PathsMatch_accepts_suffix_and_file_name()
    {
        Assert.True(CorrespondencePaths.PathsMatch("src/Foo.cs", "Foo.cs", "Foo.cs"));
        Assert.True(CorrespondencePaths.PathsMatch("repo/src/Foo.cs", "src/Foo.cs", "Foo.cs"));
        Assert.False(CorrespondencePaths.PathsMatch("other/Bar.cs", "src/Foo.cs", "Foo.cs"));
    }
}
