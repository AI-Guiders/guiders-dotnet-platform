using AIGuiders.Platform.Authoring.Command.Bundles;
using AIGuiders.Platform.Authoring.Command.Catalog;
using AIGuiders.Platform.Authoring.Core;
using Xunit;

namespace AIGuiders.Platform.Authoring.Tests;

public sealed class CatalogProjectTests
{
    [Fact]
    public void Open_builds_document_graph_with_federation_imports()
    {
        var workspace = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        var catalog = Path.Combine(workspace, "Fixtures", "Authoring", "dash.catalog");

        var result = CatalogProject.Open(workspace, catalog, CatalogBundleLibrary.Federation);

        Assert.True(result.Success);
        Assert.NotNull(result.Project);
        Assert.NotNull(result.Document);
        Assert.Equal("dash", result.Document!.Planet);

        var entry = result.Project!.Documents[0];
        Assert.Equal(AuthoringDocumentKind.LogicalFile, entry.Ref.Kind);

        var imports = result.Project.Documents
            .Where(static d => d.Ref.Kind == AuthoringDocumentKind.FederationImport)
            .Select(static d => d.Ref.Path)
            .ToList();

        Assert.Equal(result.Document.Imports.Count, imports.Count);
        Assert.All(result.Document.Imports, import => Assert.Contains(imports, p => p == import));
    }
}
