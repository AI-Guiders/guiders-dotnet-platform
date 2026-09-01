using AIGuiders.Platform.Authoring.Core;
using Xunit;

namespace AIGuiders.Platform.Authoring.Tests;

public sealed class AuthoringImportLineTests
{
    [Theory]
    [InlineData("import <grain/date-filter>", "grain/date-filter", null, false)]
    [InlineData("import <grain/date-filter> as dash-date", "grain/date-filter", "dash-date", false)]
    [InlineData("import \"diagrams/foo.dashdiagram\"", "diagrams/foo.dashdiagram", null, false)]
    [InlineData("import \"glob/*.dashdiagram\"", "glob/*.dashdiagram", null, false)]
    [InlineData("!include \"legacy.dashinclude\"", "legacy.dashinclude", null, true)]
    public void TryParse_recognizes_import_shapes(
        string line,
        string path,
        string? alias,
        bool legacy)
    {
        Assert.True(AuthoringImportLine.TryParse(line, out var directive));
        Assert.NotNull(directive);
        Assert.Equal(path, directive!.Path);
        Assert.Equal(alias, directive.Alias);
        Assert.Equal(legacy, directive.LegacyIncludeKeyword);
    }

    [Fact]
    public void TryParse_wire_uses_WireLibrary_kind()
    {
        Assert.True(AuthoringImportLine.TryParse("import <value/email>", out var directive));
        Assert.Equal(AuthoringImportTargetKind.WireLibrary, directive!.TargetKind);
    }

    [Fact]
    public void TryParse_quoted_uses_LogicalPath_kind()
    {
        Assert.True(AuthoringImportLine.TryParse("import \"a/b.catalog\"", out var directive));
        Assert.Equal(AuthoringImportTargetKind.LogicalPath, directive!.TargetKind);
    }

    [Fact]
    public void TryParse_ignores_trailing_comment()
    {
        Assert.True(AuthoringImportLine.TryParse("import <grain/foo> # bundle", out var directive));
        Assert.Equal("grain/foo", directive!.Path);
    }

    [Theory]
    [InlineData("catalog dash")]
    [InlineData("include <grain/foo>")]
    [InlineData("import grain/foo")]
    public void TryParse_rejects_non_directives(string line)
    {
        Assert.False(AuthoringImportLine.TryParse(line, out _));
    }
}
