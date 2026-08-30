#nullable enable
using AIGuiders.Platform.LanguageIntelligence.Anchors;
using AIGuiders.Platform.Notations.Bracket;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class BracketAnchorWireTests
{
    [Fact]
    public void Parse_code_family_roundtrip()
    {
        var span = BracketAnchorWire.Parse("[F:Program.cs;M:Foo;L:10]");
        Assert.Equal(BracketAxisFamily.Csharp, BracketAnchorWire.ClassifyFamily(span, out var error));
        Assert.Null(error);
        Assert.Contains("F:Program.cs", BracketAnchorWire.Format(span));
    }

    [Fact]
    public void Parse_navigation_nested_anchor()
    {
        var span = BracketAnchorWire.Parse("[Family:navigation;Command:open;Anchor:[F:README.md;L:10]]");
        Assert.Equal(BracketAxisFamily.Navigation, BracketAnchorWire.ClassifyFamily(span, out _));
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
