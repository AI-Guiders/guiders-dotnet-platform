using AIGuiders.Platform.Authoring.Command.Catalog;
using AIGuiders.Platform.Authoring.Conformance;
using AIGuiders.Platform.Authoring.Core;
using AIGuiders.Platform.CommandPlane.Catalog.CodeGen;

namespace AIGuiders.Platform.Tests.Authoring;

public sealed class CatalogParserTests
{
    [Fact]
    public void Parse_dash_catalog_fixture_has_planet_and_commands()
    {
        var text = LoadFixture("dash.catalog");
        var result = CatalogParser.Parse(text);

        Assert.NotNull(result.Document);
        Assert.Equal("dash", result.Document!.Planet);
        Assert.Contains(result.Document.Commands, c => c.Command == "filter.date");
        Assert.Contains(result.Document.Defaults.CommandSurfaces, s => s == "slash.bar");
        Assert.Equal("command-console", result.Document.Channels.First(c => c.Sub == "filter").CommandNotation);
    }

    [Fact]
    public void Notation_mismatch_is_compile_error()
    {
        var text = LoadFixture("notation-mismatch.catalog");
        var result = CatalogConformance.ValidateDocument(text);

        Assert.Contains(
            result.Diagnostics,
            d => d.Code == AuthoringDiagnosticCode.NotationWireMismatch);
    }

    [Fact]
    public void Mcp_emitter_writes_tools_for_exposed_commands()
    {
        var text = LoadFixture("dash.catalog");
        var doc = CatalogParser.Parse(text).Document!;
        var json = CatalogMcpToolsEmitter.EmitJson(doc);

        Assert.Contains("dash.filter.date", json, StringComparison.Ordinal);
    }

    static string LoadFixture(string name)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "Authoring", name);
        return File.ReadAllText(path);
    }
}
