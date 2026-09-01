using AIGuiders.Platform.Authoring.Deck;
using Xunit;

namespace AIGuiders.Platform.Authoring.Tests;

public sealed class DeckParserTests
{
    [Fact]
    public void Parse_dashspec_studio_fixture_has_preset_zones_topology()
    {
        var text = LoadFixture("dashspec-studio.deck");
        var result = DeckParser.Parse(text, "dashspec-studio.deck");

        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.Document);
        Assert.Equal("dashspec-studio", result.Document!.Planet);

        var preset = Assert.Single(result.Document.Presets);
        Assert.Equal("report-author", preset.Name);
        Assert.Equal("(MFD)(F)", preset.TopologyWire);
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
