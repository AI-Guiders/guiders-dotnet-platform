#nullable enable

using AIGuiders.Platform.Execution.Documentation.LinkMutate;
using AIGuiders.Platform.Modeling.Notations.Bracket;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class DocAnchorRenamerTests
{
    [Fact]
    public void ApplyRename_patches_member_axis_in_fixture_copy()
    {
        var source = FixturePath("doc-anchor-rename.fixture.md");
        var tempDir = Path.Combine(Path.GetTempPath(), "guiders-doc-rename-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        var target = Path.Combine(tempDir, "rename.md");
        File.Copy(source, target);

        var result = DocAnchorRenamer.ApplyRename([target], "OldMember", "NewMember", DocSymbolRenameKind.Member);

        Assert.Equal(1, result.FilesChanged);
        Assert.Equal(1, result.WiresChanged);
        var text = File.ReadAllText(target);
        Assert.Contains("Member:NewMember", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Member:OldMember", text, StringComparison.Ordinal);
        Assert.Contains("Type:OldTypeName", text, StringComparison.Ordinal);

        Directory.Delete(tempDir, recursive: true);
    }

    [Fact]
    public void PatchText_patches_type_axis_only()
    {
        var text = File.ReadAllText(FixturePath("doc-anchor-rename.fixture.md"));
        var wires = 0;
        var patched = DocAnchorRenamer.PatchText(
            text,
            BracketProfiles.DocSymbol,
            "OldTypeName",
            "NewTypeName",
            DocSymbolRenameKind.Type,
            ref wires);

        Assert.Equal(1, wires);
        Assert.Contains("Type:NewTypeName", patched, StringComparison.Ordinal);
        Assert.Contains("Member:OldMember", patched, StringComparison.Ordinal);
    }

    static string FixturePath(string name) =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "Fixtures", "Documentation", name));
}
