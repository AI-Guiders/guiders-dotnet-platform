using System.Text.Json;
using AIGuiders.Platform.Authoring.Command.Bundles;
using AIGuiders.Platform.Authoring.Command.Catalog;
using AIGuiders.Platform.Authoring.Core;
using Xunit;

namespace AIGuiders.Platform.Authoring.Tests;

public sealed class ProjectConformanceSpecTests
{
    [Fact]
    public void Import_graph_spec_vectors()
    {
        var root = FindConformanceRoot();
        var json = File.ReadAllText(Path.Combine(root, "project", "import-graph.spec.json"));
        using var doc = JsonDocument.Parse(json);

        foreach (var vector in doc.RootElement.GetProperty("vectors").EnumerateArray())
        {
            var id = vector.GetProperty("id").GetString()!;
            switch (id)
            {
                case "import-line-wire":
                case "import-line-logical":
                case "legacy-include-alias":
                    AssertImportLineVector(vector);
                    break;
                case "catalog-project-federation-graph":
                    AssertCatalogProjectVector(vector);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown vector id: {id}");
            }
        }
    }

    static void AssertImportLineVector(JsonElement vector)
    {
        var line = vector.GetProperty("line").GetString()!;
        var expect = vector.GetProperty("expect");

        Assert.True(AuthoringImportLine.TryParse(line, out var directive));
        Assert.NotNull(directive);
        Assert.Equal(expect.GetProperty("targetKind").GetString(), directive!.TargetKind.ToString());
        Assert.Equal(expect.GetProperty("path").GetString(), directive.Path);

        if (expect.TryGetProperty("alias", out var alias))
        {
            Assert.Equal(alias.GetString(), directive.Alias);
        }

        if (expect.TryGetProperty("legacy", out var legacy))
        {
            Assert.Equal(legacy.GetBoolean(), directive.LegacyIncludeKeyword);
        }
    }

    static void AssertCatalogProjectVector(JsonElement vector)
    {
        var fixture = vector.GetProperty("catalogFile").GetString()!;
        var expect = vector.GetProperty("expect");
        var workspace = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        var catalog = Path.Combine(workspace, "Fixtures", "Authoring", fixture);

        var result = CatalogProject.Open(workspace, catalog, CatalogBundleLibrary.Federation);
        Assert.True(result.Success);
        Assert.Equal(expect.GetProperty("planet").GetString(), result.Document!.Planet);

        var logical = result.Project!.Documents.Count(d => d.Ref.Kind == AuthoringDocumentKind.LogicalFile);
        var federation = result.Project.Documents.Count(d => d.Ref.Kind == AuthoringDocumentKind.FederationImport);
        Assert.Equal(expect.GetProperty("logicalDocuments").GetInt32(), logical);
        Assert.True(federation >= expect.GetProperty("federationImportsMin").GetInt32());
    }

    static string FindConformanceRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "docs", "conformance", "authoring");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException("authoring conformance root not found");
    }
}
