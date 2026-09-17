#nullable enable
using AIGuiders.Platform.Execution.LanguageIntelligence.Relations;
using AIGuiders.Platform.Modeling.Notations.Bracket;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class BracketRelationWireTests
{
    [Fact]
    public void Parse_code_family_roundtrip()
    {
        var span = BracketRelationWire.Parse("[F:Program.cs;M:Foo;L:10]");
        Assert.Equal(BracketAxisFamily.Csharp, BracketRelationWire.ClassifyFamily(span, out var error));
        Assert.Null(error);
        Assert.Contains("F:Program.cs", BracketRelationWire.Format(span));
    }

    [Fact]
    public void Parse_navigation_nested_anchor()
    {
        var span = BracketRelationWire.Parse("[Family:navigation;Command:open;Anchor:[F:README.md;L:10]]");
        Assert.Equal(BracketAxisFamily.Navigation, BracketRelationWire.ClassifyFamily(span, out _));
        Assert.NotNull(span.NestedAnchor);
        Assert.Equal("README.md", span.NestedAnchor!.File);
    }

    [Fact]
    public void EnvelopeScan_finds_nested_outer_only()
    {
        const string text = "See [Anchor:[F:a.cs;M:B]] here";
        var envelopes = BracketEnvelopeScan.LocateInText(text);
        Assert.Single(envelopes);
        Assert.Equal("Anchor:[F:a.cs;M:B]", envelopes[0].Inner);
        Assert.Equal("[Anchor:[F:a.cs;M:B]]", envelopes[0].Wire);
    }
}
