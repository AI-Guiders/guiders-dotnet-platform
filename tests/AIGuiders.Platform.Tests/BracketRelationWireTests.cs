#nullable enable

using AIGuiders.Platform.Execution.LanguageIntelligence;
using AIGuiders.Platform.Execution.LanguageIntelligence.Relations;
using AIGuiders.Platform.Notations.Bracket;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class RelationWireBoundaryTests
{
    [Fact]
    public void DocScan_parses_code_family_axes()
    {
        Assert.True(RelationWireBoundary.TryParseDocScan("[F:Program.cs;M:Foo;L:10]", out var axes, out var error), error);
        Assert.Equal("Program.cs", axes.File);
        Assert.Equal("Foo", axes.MemberKey);
        Assert.Equal(10, axes.LineStart);
        Assert.Equal(BracketAxisFamily.Csharp, WireFamilyClassifier.Classify(WireFamilyClassifier.Probe.FromCodeEdit(axes), out error));
        Assert.Null(error);
    }

    [Fact]
    public void DocScan_rejects_navigation_wires()
    {
        Assert.False(RelationWireBoundary.TryParseDocScan(
            "[Family:navigation;Command:open;Anchor:[F:README.md;L:10]]",
            out _,
            out _));
        Assert.True(LegacyNavWireIngest.TryParse(
            "[Family:navigation;Command:open;Anchor:[F:README.md;L:10]]",
            out var nav,
            out _));
        Assert.Equal("README.md", nav.File);
        Assert.Equal(10, nav.Line);
    }

    [Fact]
    public void Kind_first_parse_boundary_matches_doc_scan()
    {
        const string wire = "[Kind:CodeEdit; File:Program.cs; Member:Foo]";
        var spec = RelationSpecWireBoundary.TryParseKindSpec(wire);
        Assert.NotNull(spec);
        Assert.True(CodeEditResolveProjection.TryFromRelationSpec(spec, out var kindAxes));

        Assert.True(RelationWireBoundary.TryParseDocScan("[F:Program.cs;M:Foo]", out var docAxes, out _));
        Assert.Equal(docAxes.File, kindAxes.File);
        Assert.Equal(docAxes.MemberKey, kindAxes.MemberKey);
    }

    [Fact]
    public void CodeEditResolveProjection_maps_xml_wire_encoding_to_axes()
    {
        const string wire = "[Kind:CodeEdit; File:doc.xml; Element:Root/Item]";
        var spec = RelationSpecWireBoundary.TryParseKindSpec(wire);
        Assert.NotNull(spec);
        Assert.True(CodeEditResolveProjection.TryFromRelationSpec(spec, out var axes));
        Assert.Equal("doc.xml", axes.File);
        Assert.Equal("Root/Item", axes.XmlPath);
        Assert.Null(axes.MemberKey);
    }

    [Fact]
    public void BracketAxisFamily_includes_Json_equals_5()
    {
        Assert.Equal(0, (int)BracketAxisFamily.None);
        Assert.Equal(5, (int)BracketAxisFamily.Json);
    }

    [Fact]
    public void LegacyNavWireIngest_flattens_nested_anchor()
    {
        Assert.True(LegacyNavWireIngest.TryParse(
            "[Family:navigation;Command:open;Anchor:[File:a.fs;Member:inner]]",
            out var nav,
            out _));
        Assert.Equal("a.fs", nav.File);
        Assert.Equal("inner", nav.Member);
        Assert.Equal("open", nav.Command);
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
