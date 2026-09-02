using AIGuiders.Platform.Authoring.Deck;
using AIGuiders.Platform.IntermediateRepresentation.Presentation;
using Xunit;

namespace AIGuiders.Platform.Authoring.Tests;

public sealed class DeckParserTests
{
    [Fact]
    public void Parse_dashspec_studio_fixture_has_typed_topology()
    {
        var text = LoadFixture("dashspec-studio.deck.gdl");
        var result = DeckParser.Parse(text, "dashspec-studio.deck.gdl");

        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.Document);
        Assert.Equal("dashspec-studio", result.Document!.Planet);

        var preset = Assert.Single(result.Document.Presets);
        Assert.Equal("report-author", preset.Name);
        Assert.NotNull(preset.Topology);
        Assert.Equal("(MFD)(F)", preset.Topology!.SourceWire);
        Assert.Equal(TopologyArrangement.MultiHost, preset.Topology.Arrangement);
        Assert.Equal(2, preset.Topology.HostCount);
        Assert.Equal(AttentionDisplayRole.Mfd, preset.Topology.Hosts[0].Role);
        Assert.Equal(AttentionDisplayRole.Forward, preset.Topology.Hosts[1].Role);
        Assert.Equal("report-preview", preset.ForwardZoneId);
        Assert.Equal(["spec-tree", "resolve"], preset.MfdZoneIds);
        Assert.Equal("when alerts", preset.EicasPolicy);

        Assert.Equal("forward", result.Document.ZoneBindings["report-preview"]);
        Assert.Equal("forward", result.Document.ZoneBindings["repl"]);
        Assert.Equal("mfd", result.Document.ZoneBindings["spec-tree"]);
    }

    static string LoadFixture(string name)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "Authoring", name);
        return File.ReadAllText(path);
    }
}
