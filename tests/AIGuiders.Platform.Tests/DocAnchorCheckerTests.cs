#nullable enable

using AIGuiders.Platform.Execution.Documentation.Anchors;
using AIGuiders.Platform.Execution.Documentation.LinkCheck;
using AIGuiders.Platform.Modeling.Notations.Bracket;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class DocAnchorCheckerTests
{
    [Fact]
    public void CheckMarkdownRoots_reports_missing_type_and_member()
    {
        var fixture = FixturePath("doc-anchor-check.fixture.md");
        var failures = DocAnchorChecker.CheckMarkdownRoots([fixture], new StubCatalog());

        Assert.Contains(failures, f => f.Contains("type_not_found:MissingType", StringComparison.Ordinal));
        Assert.Contains(failures, f => f.Contains("member_not_found:OkType.MissingMember", StringComparison.Ordinal));
        Assert.DoesNotContain(failures, f => f.Contains("OkType", StringComparison.Ordinal) && f.Contains("type_not_found", StringComparison.Ordinal));
    }

    [Fact]
    public void DocAnchorWire_format_roundtrips_doc_profile()
    {
        Assert.True(
            BracketReader.Default.TryRead(
                "[Family:doc; Package:Notations.Argument; Type:NormalizedArguments; Member:ReaderId]",
                BracketProfiles.DocSymbol,
                BracketAxisValuePlans.DocSymbol,
                out var wire,
                out var error),
            error);

        var formatted = DocAnchorWire.Format(wire!);
        Assert.Equal(
            "[Family:doc; Package:Notations.Argument; Type:NormalizedArguments; Member:ReaderId]",
            formatted);
    }

    static string FixturePath(string name) =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "Fixtures", "Documentation", name));

    sealed class StubCatalog : IDocSymbolCatalog
    {
        public bool TypeExists(string typeName, string? packageHint) =>
            typeName.Equals("OkType", StringComparison.Ordinal);

        public bool MemberExists(string typeName, string memberName, string? packageHint) =>
            typeName.Equals("OkType", StringComparison.Ordinal)
            && memberName.Equals("OkMember", StringComparison.Ordinal);
    }
}
