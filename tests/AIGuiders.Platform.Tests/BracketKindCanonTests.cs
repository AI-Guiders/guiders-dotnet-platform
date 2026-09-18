#nullable enable
using Microsoft.FSharp.Core;
using AIGuiders.Platform.Modeling.LanguageIntelligence.Relations;
using AIGuiders.Platform.Modeling.Notations.Bracket;
using AIGuiders.Platform.Execution.LanguageIntelligence.Relations;
using AIGuiders.Platform.Notations.Bracket;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class BracketKindCanonTests
{
    [Fact]
    public void Kind_CodeEdit_parses_canonical_axes()
    {
        Assert.True(
            BracketReader.Default.TryRead(
                "[Kind:CodeEdit; File:Program.cs; Member:Foo]",
                BracketProfiles.CdpSquareKeyValue,
                out var wire,
                out var error),
            error);

        var spec = BracketRelationWire.tryParseRelationSpec(wire!);
        Assert.NotNull(spec);
    }

    [Fact]
    public void Kind_Nav_parses_file_and_command()
    {
        Assert.True(
            BracketReader.Default.TryRead(
                "[Kind:Nav; File:README.md; Command:open]",
                BracketProfiles.CdpSquareKeyValue,
                out var wire,
                out var error),
            error);

        var spec = BracketRelationWire.tryParseRelationSpec(wire!);
        Assert.NotNull(spec);
    }

    [Fact]
    public void Legacy_F_M_L_wire_does_not_emit_Kind_spec()
    {
        Assert.True(
            BracketReader.Default.TryRead(
                "[F:Program.cs; M:Foo; L:10]",
                BracketProfiles.CdpSquareKeyValue,
                out var wire,
                out var error),
            error);

        Assert.Null(BracketRelationWire.tryParseRelationSpec(wire!));
    }

    [Fact]
    public void Kind_CodeEdit_line_parses_and_projects()
    {
        Assert.True(
            BracketReader.Default.TryRead(
                "[Kind:CodeEdit; File:Program.cs; Member:Foo; Line:10]",
                BracketProfiles.CdpSquareKeyValue,
                out var wire,
                out var error),
            error);

        var parsed = BracketRelationWire.tryParseRelationSpec(wire!);
        Assert.NotNull(parsed);
        Assert.True(FSharpOption<RelationSpec>.get_IsSome(parsed!));
        Assert.True(CodeEditResolveProjection.TryFromRelationSpec(parsed!.Value, out var axes));
        Assert.Equal("Program.cs", axes.File);
        Assert.Equal("Foo", axes.MemberKey);
        Assert.Equal(10, axes.LineStart);
        Assert.Equal(10, axes.LineEnd);
    }
}
