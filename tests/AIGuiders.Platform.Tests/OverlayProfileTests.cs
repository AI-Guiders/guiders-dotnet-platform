#nullable enable
using AIGuiders.Platform.Combinations;
using AIGuiders.Platform.Combinations.Overlay;
using AIGuiders.Platform.Combinations.Workspace;
using AIGuiders.Platform.Configurations.Workspace;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class OverlayProfileTests
{
    [Fact]
    public void WorkspaceOverlay_profile_has_named_semantics()
    {
        Assert.Equal("workspace.field-overlay", WorkspaceOverlay.FieldOverlay.Name);
        Assert.Equal(CombinationSemantics.FieldOverlay, WorkspaceOverlay.FieldOverlay.Semantics);
    }

    [Fact]
    public void WorkspaceOverlay_field_overlay_matches_extension_merge()
    {
        var baseline = new WorkspaceDocument
        {
            Workspace = new WorkspaceSection
            {
                Adr = new WorkspaceAdrSettings { RootDir = "docs/adr", MaxRelated = 3 },
            },
        };
        var overlay = new WorkspaceDocument
        {
            Workspace = new WorkspaceSection
            {
                Adr = new WorkspaceAdrSettings { MaxRelated = 7 },
            },
        };

        var viaPolicy = WorkspaceOverlay.FieldOverlay.Combinator(baseline, overlay);
        var viaExtension = baseline.MergeOver(overlay);

        Assert.Equal(viaPolicy.Workspace!.Adr!.RootDir, viaExtension.Workspace!.Adr!.RootDir);
        Assert.Equal(7, viaExtension.Workspace.Adr.MaxRelated);
        Assert.Equal("docs/adr", viaExtension.Workspace.Adr.RootDir);
    }

    [Fact]
    public void WorkspaceOverlay_replace_whole_features_section()
    {
        var baseline = new WorkspaceDocument
        {
            Workspace = new WorkspaceSection
            {
                Features = new WorkspaceFeatures
                {
                    Feature = [new WorkspaceFeature { Id = "ship" }],
                },
            },
        };
        var overlay = new WorkspaceDocument
        {
            Workspace = new WorkspaceSection
            {
                Features = new WorkspaceFeatures
                {
                    Feature = [new WorkspaceFeature { Id = "user" }],
                },
            },
        };

        var merged = baseline.MergeOver(overlay);
        Assert.Single(merged.Workspace!.Features!.Feature);
        Assert.Equal("user", merged.Workspace.Features.Feature[0].Id);
    }

    [Fact]
    public void Custom_profile_builds_combinator_via_fluent_rules()
    {
        var policy = OverlayProfile.For<DemoPair>("demo.pair-overlay", CombinationSemantics.FieldOverlay)
            .Rule(static (baseline, overlay) => new DemoPair
            {
                A = baseline.A + overlay.A,
                B = baseline.B + overlay.B,
            })
            .Build();

        var result = policy.Combinator(new DemoPair { A = 1, B = 2 }, new DemoPair { A = 10, B = 20 });
        Assert.Equal(11, result.A);
        Assert.Equal(22, result.B);
    }

    sealed class DemoPair
    {
        public int A { get; set; }
        public int B { get; set; }
    }
}
