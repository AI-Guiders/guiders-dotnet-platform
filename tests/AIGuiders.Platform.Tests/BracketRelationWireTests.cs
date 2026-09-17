#nullable enable
using AIGuiders.Platform.Execution.LanguageIntelligence.Relations;
using AIGuiders.Platform.Modeling.Notations.Bracket;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class RelationWireBoundaryTests
{
    [Fact]
    public void Parse_code_family_roundtrip()
    {
        var span = RelationWireBoundary.Parse("[F:Program.cs;M:Foo;L:10]");
        Assert.Equal(BracketAxisFamily.Csharp, RelationWireBoundary.ClassifyFamily(span, out var error));
        Assert.Null(error);
        Assert.Contains("F:Program.cs", RelationWireBoundary.Format(span));
    }

    [Fact]
    public void Parse_navigation_nested_anchor()
    {
        var span = RelationWireBoundary.Parse("[Family:navigation;Command:open;Anchor:[F:README.md;L:10]]");
        Assert.Equal(BracketAxisFamily.Navigation, RelationWireBoundary.ClassifyFamily(span, out _));
        Assert.NotNull(span.NestedAnchor);
        Assert.Equal("README.md", span.NestedAnchor!.File);
    }

    [Fact]
    public void Kind_first_parse_boundary_matches_legacy_span()
    {
        const string wire = "[Kind:CodeEdit; File:Program.cs; Member:Foo]";
        var spec = RelationSpecWireBoundary.TryParseKindSpec(wire);
        Assert.NotNull(spec);
        Assert.True(AIGuiders.Platform.Execution.LanguageIntelligence.Relations.RelationSpecLegacyBridge.TryToLegacySpan(spec, out var kindSpan));

        var legacySpan = RelationWireBoundary.Parse("[F:Program.cs;M:Foo]");
        Assert.Equal(legacySpan.File, kindSpan.File);
        Assert.Equal(legacySpan.MemberKey, kindSpan.MemberKey);
    }

    [Fact]
    public void Obsolete_LegacyBracketRelationWire_delegates_to_RelationWireBoundary()
    {
#pragma warning disable CS0618
        var span = LegacyBracketRelationWire.Parse("[F:a.cs;M:B]");
#pragma warning restore CS0618
        Assert.Equal("a.cs", span.File);
        Assert.Equal("B", span.MemberKey);
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
