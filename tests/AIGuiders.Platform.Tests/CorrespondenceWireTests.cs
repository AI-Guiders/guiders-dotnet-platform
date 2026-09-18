#nullable enable

using AIGuiders.Platform.Execution.Documentation.Correspondence;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class CorrespondenceWireTests
{
    [Fact]
    public void BuildCodeEdit_emits_kind_canon_wire()
    {
        var wire = CorrespondenceWire.BuildCodeEdit("src/Foo.cs", 10, 10, null);
        Assert.Equal("[Kind:CodeEdit; File:src/Foo.cs; Line:10]", wire);
    }

    [Fact]
    public void BuildCodeEdit_member_wins_over_line_span()
    {
        var wire = CorrespondenceWire.BuildCodeEdit("src/Foo.cs", 10, 20, "Bar");
        Assert.Equal("[Kind:CodeEdit; File:src/Foo.cs; Member:Bar]", wire);
    }

    [Fact]
    public void Build_legacy_profile_still_parses_doc_reverse_scan()
    {
        var wire = CorrespondenceWire.Build("docs/adr/x.md", 10, 20, null);
        Assert.True(CorrespondenceWire.TryParseBracket(wire, out var file, out var ls, out var le, out var member));
        Assert.Equal("docs/adr/x.md", file);
        Assert.Equal(10, ls);
        Assert.Equal(10, le);
        Assert.Null(member);
    }
}
