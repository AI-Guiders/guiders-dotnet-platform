#nullable enable
using AIGuiders.Platform.Configurations.Workspace;
using AIGuiders.Platform.Sources;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class SourceMergeTests
{
    [Fact]
    public void Merge_combinator_overlays_in_layer_order()
    {
        var baseline = SourceCatalog.From(
            new WorkspaceDocument
            {
                Workspace = new WorkspaceSection
                {
                    Adr = new WorkspaceAdrSettings { RootDir = "docs/adr", MaxRelated = 3 },
                },
            },
            "baseline");

        var overlay = SourceCatalog.From(
            new WorkspaceDocument
            {
                Workspace = new WorkspaceSection
                {
                    Adr = new WorkspaceAdrSettings { MaxRelated = 7 },
                },
            },
            "overlay");

        var merged = WorkspaceSources.MergeOverlay(baseline, overlay, "merged");
        var doc = merged.Load();

        Assert.Equal("merged", merged.SourceId);
        Assert.Equal("docs/adr", doc.Workspace!.Adr!.RootDir);
        Assert.Equal(7, doc.Workspace.Adr.MaxRelated);
    }

    [Fact]
    public void Merge_three_layers_accumulates()
    {
        var layers = new ISource<WorkspaceDocument>[]
        {
            SourceCatalog.From(new WorkspaceDocument
            {
                Workspace = new WorkspaceSection { Adr = new WorkspaceAdrSettings { RootDir = "a" } },
            }, "l0"),
            SourceCatalog.From(new WorkspaceDocument
            {
                Workspace = new WorkspaceSection { Adr = new WorkspaceAdrSettings { MaxRelated = 2 } },
            }, "l1"),
            SourceCatalog.From(new WorkspaceDocument
            {
                Workspace = new WorkspaceSection
                {
                    Features = new WorkspaceFeatures
                    {
                        Feature = [new WorkspaceFeature { Id = "x" }],
                    },
                },
            }, "l2"),
        };

        var merged = SourceCatalog.Merge(layers, static (b, o) => b.MergeOver(o));
        var doc = merged.Load();

        Assert.Equal("a", doc.Workspace!.Adr!.RootDir);
        Assert.Equal(2, doc.Workspace.Adr.MaxRelated);
        Assert.Single(doc.Workspace.Features!.Feature);
        Assert.Equal("x", doc.Workspace.Features.Feature[0].Id);
    }
}
