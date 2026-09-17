#nullable enable
using AIGuiders.Platform.Execution.LanguageIntelligence;
using AIGuiders.Platform.Execution.LanguageIntelligence.Relations;
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
        Assert.True(RelationSpecLegacyBridge.TryToLegacySpan(spec, out var kindSpan));

        var legacySpan = RelationWireBoundary.Parse("[F:Program.cs;M:Foo]");
        Assert.Equal(legacySpan.File, kindSpan.File);
        Assert.Equal(legacySpan.MemberKey, kindSpan.MemberKey);
    }

    [Fact]
    public void RelationSpecLegacyBridge_maps_xml_wire_encoding_to_span()
    {
        const string wire = "[Kind:CodeEdit; File:doc.xml; Element:Root/Item]";
        var spec = RelationSpecWireBoundary.TryParseKindSpec(wire);
        Assert.NotNull(spec);
        Assert.True(RelationSpecLegacyBridge.TryToLegacySpan(spec, out var span));
        Assert.Equal("doc.xml", span.File);
        Assert.Equal("Root/Item", span.XmlPath);
        Assert.Null(span.MemberKey);
    }

    [Fact]
    public void BracketAxisFamily_includes_Json_equals_5()
    {
        Assert.Equal(0, (int)BracketAxisFamily.None);
        Assert.Equal(5, (int)BracketAxisFamily.Json);
    }

    [Fact]
    public void BracketAnchorSpan_supports_nested_anchors()
    {
        var inner = new BracketAnchorSpan(null, "inner", null, null);
        var outer = new BracketAnchorSpan("a.fs", null, null, null, NestedAnchor: inner);
        Assert.Same(inner, outer.NestedAnchor);
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
