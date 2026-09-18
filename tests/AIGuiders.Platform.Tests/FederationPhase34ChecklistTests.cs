using AIGuiders.Platform.Execution.CommandPlane.Catalog;
using AIGuiders.Platform.Execution.Configurations.Workspace;
using AIGuiders.Platform.Execution.Documentation.Correspondence;
using AIGuiders.Platform.Execution.Ide.Session;
using Xunit;

namespace AIGuiders.Platform.Tests;

/// <summary>Plan §10 Phase 3–4 verification gate for Execution IO + attach contract wiring.</summary>
public sealed class FederationPhase34ChecklistTests
{
    [Fact]
    public void Execution_IO_sources_and_attach_catalog_types_are_public()
    {
        Assert.True(typeof(DotNetSlnxGraphSources).IsPublic);
        Assert.True(typeof(WorkspaceGraphSources).IsPublic);
        Assert.True(typeof(SessionContentsLoader).IsPublic);
        Assert.True(typeof(KnowledgeWireSources).IsPublic);
        Assert.True(typeof(GoldenEvidence).IsPublic);
        Assert.True(typeof(FederationAttachCatalog).IsPublic);
    }

    [Fact]
    public void Relations_props_bundle_exists_on_disk()
    {
        var props = FindRepoFile("eng", "Guiders.Modeling.relations.props");
        Assert.True(File.Exists(props), props);
    }

    static string FindRepoFile(params string[] parts)
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            var candidate = Path.Combine([dir.FullName, .. parts]);
            if (File.Exists(candidate))
                return candidate;
        }

        throw new InvalidOperationException($"Could not locate {string.Join('/', parts)}.");
    }
}
