using AIGuiders.Platform.Authoring.Command.Bundles;
using AIGuiders.Platform.Authoring.Command.Catalog;
using AIGuiders.Platform.Authoring.Conformance;
using AIGuiders.Platform.Authoring.Core;
using AIGuiders.Platform.CommandPlane.Catalog.CodeGen;
using AIGuiders.Platform.IntermediateRepresentation.Command;

using Xunit;

namespace AIGuiders.Platform.Authoring.Tests;

public sealed class CatalogParserTests
{
    [Fact]
    public void Parse_dash_catalog_fixture_has_planet_and_commands()
    {
        var text = LoadFixture("dash.catalog");
        var result = CatalogParser.Parse(text, bundleLibrary: CatalogBundleLibrary.Federation);

        Assert.NotNull(result.Document);
        Assert.Equal("dash", result.Document!.Planet);
        Assert.Contains(result.Document.Commands, c => c.Command == "filter.date");
        Assert.Contains(result.Document.Defaults.CommandSurfaces, s => s == "slash.bar");
        Assert.Equal("command-console", result.Document.Channels.First(c => c.Sub == "filter").CommandGrammar);

        var dateProfile = result.Document.Profiles.First(p => p.Name == "date-value");
        Assert.Contains(dateProfile.Entries, e => e.Entry == "preset" && e.Ref == "today");
        Assert.Contains(dateProfile.Entries, e => e.Entry == "constructor" && e.Ref == "range");

        var argTail = CatalogArgTailMapper.ToArgTailProfile(dateProfile);
        Assert.Equal("date-value", argTail.Name);
        Assert.Contains(argTail.Menu, m => m.Kind == ArgTailEntryKind.Constructor && m.Ref == "range");
    }

    [Fact]
    public void KvDesugar_profiles_dotted_keys()
    {
        var body = AuthoringSource.FromText(
            """
              date-value.value.preset = today
              date-value.value.constructor = range
            """);

        var rows = KvDesugar.ProfileRows(body);
        Assert.Equal(2, rows.Count);
        Assert.Equal("date-value", rows[0]["profile"]);
        Assert.Equal("preset", rows[0]["entry"]);
        Assert.Equal("today", rows[0]["ref"]);
    }

    [Fact]
    public void Grammar_mismatch_is_compile_error()
    {
        var text = LoadFixture("grammar-mismatch.catalog");
        var result = CatalogConformance.ValidateDocument(text);

        Assert.Contains(
            result.Diagnostics,
            d => d.Code == AuthoringDiagnosticCode.GrammarWireMismatch);
    }

    [Fact]
    public void Mcp_emitter_writes_tools_for_exposed_commands()
    {
        var text = LoadFixture("dash.catalog");
        var doc = CatalogParser.Parse(text, bundleLibrary: CatalogBundleLibrary.Federation).Document!;
        var json = CatalogMcpToolsEmitter.EmitJson(doc);

        Assert.Contains("dash.filter.date", json, StringComparison.Ordinal);
    }

    [Fact]
    public void Catalog_emitter_writes_federation_surfaces()
    {
        var text = LoadFixture("dash.catalog");
        var doc = CatalogParser.Parse(text, bundleLibrary: CatalogBundleLibrary.Federation).Document!;
        var code = CatalogCatalogEmitter.EmitCSharp(doc, "DashSpec.Generated", "DashCatalog");

        Assert.Contains("slash.bar", code, StringComparison.Ordinal);
        Assert.Contains("dash.filter.date", code, StringComparison.Ordinal);
    }

    static string LoadFixture(string name)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "Authoring", name);
        return File.ReadAllText(path);
    }
}
