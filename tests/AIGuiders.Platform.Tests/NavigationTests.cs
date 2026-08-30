#nullable enable
using AIGuiders.Platform.Conformance.Navigation;
using AIGuiders.Platform.Conformance.Schemas;
using AIGuiders.Platform.Navigation;
using AIGuiders.Platform.Navigation.Code;
using AIGuiders.Platform.Navigation.Policy;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class NavigationTests
{
    [Fact]
    public void Preset_merge_peers_only_yields_include_partial_and_project_peer()
    {
        var (inc, exc, err) = NavigationPresetMerge.Merge("peers_only", null, null);
        Assert.Null(err);
        Assert.NotNull(inc);
        Assert.Contains(NavigationRelatedKinds.PartialPeer, inc);
        Assert.Contains(NavigationRelatedKinds.ProjectPeer, inc);
        Assert.Equal(2, inc!.Count);
        Assert.Empty(exc!);
    }

    [Fact]
    public void Preset_merge_unknown_preset_returns_error()
    {
        var (_, _, err) = NavigationPresetMerge.Merge("no_such_preset", null, null);
        Assert.NotNull(err);
        Assert.Contains("Неизвестный пресет", err, StringComparison.Ordinal);
    }

    [Fact]
    public void Kind_filter_unions_preset_and_request_exclude()
    {
        var (_, exc, err) = NavigationPresetMerge.Merge(
            "explore_default",
            null,
            ["same_directory"]);
        Assert.Null(err);
        Assert.Contains(NavigationRelatedKinds.ProjectPeer, exc!);
        Assert.Contains(NavigationRelatedKinds.SameDirectory, exc!);
    }

    [Fact]
    public void Explore_default_preset_excludes_project_peer()
    {
        const string wire = """
            {
              "mode": "related",
              "anchor_path": "src/Widget.cs",
              "items": [
                { "path": "src/Widget.Part.cs", "kind": "partial_peer" },
                { "path": "src/Other.cs", "kind": "project_peer" },
                { "path": "src/WidgetTests.cs", "kind": "test_counterpart" }
              ]
            }
            """;

        var scene = NavigationCodeExplorer.ExploreRelatedFromWire(wire, NavigationProfile.ExploreDefault);

        Assert.Equal(NavigationSchemes.SceneV1, scene.Schema);
        Assert.Equal(3, scene.Nodes.Count);
        Assert.DoesNotContain(scene.Nodes, n => n.Kind == "project_peer");
        Assert.Contains(scene.Nodes, n => n.Kind == "partial_peer");
    }

    [Fact]
    public void Same_directory_kind_cap_limits_neighbors()
    {
        const string wire = """
            {
              "mode": "related",
              "anchor_path": "src/Alpha.cs",
              "items": [
                { "path": "src/Beta.cs", "kind": "same_directory" },
                { "path": "src/Gamma.cs", "kind": "same_directory" },
                { "path": "src/Delta.cs", "kind": "same_directory" },
                { "path": "src/Epsilon.cs", "kind": "same_directory" },
                { "path": "src/Zeta.cs", "kind": "same_directory" }
              ]
            }
            """;

        var scene = NavigationCodeExplorer.ExploreRelatedFromWire(
            wire,
            new NavigationProfile { MaxRelated = 8 });

        var sameDir = scene.Nodes.Count(n => n.Kind == "same_directory");
        Assert.Equal(4, sameDir);
        Assert.Equal(5, scene.Nodes.Count);
    }

    [Fact]
    public void InMemory_explorer_finds_test_counterpart()
    {
        var anchor = new NavigationAnchor(Path.GetFullPath("src/Widget.cs"));
        var files = new[]
        {
            anchor.Path,
            Path.GetFullPath("src/WidgetTests.cs"),
            Path.GetFullPath("src/Other.cs"),
        };

        var scene = NavigationCodeExplorer.ExploreRelatedInMemory(
            anchor,
            files,
            NavigationProfile.ExploreDefault);

        Assert.Contains(scene.Nodes, n => n.Kind == "test_counterpart");
    }
}

public sealed class NavigationSpecConformanceTests
{
    [Fact]
    public void Docs_navigation_spec_passes_schema_and_harness()
    {
        var path = Path.Combine(FindConformanceRoot(), "navigation", "code-explore-scene.spec.json");
        var json = File.ReadAllText(path);
        var schemaErrors = ConformanceSchemaValidator.ValidateNavigationJson(json);
        Assert.True(schemaErrors.Count == 0, string.Join(Environment.NewLine, schemaErrors));

        var spec = NavigationSpecLoader.LoadJson(json);
        var errors = NavigationSpecConformance.ValidateDocument(spec);
        Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));
    }

    static string FindConformanceRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "docs", "conformance");
            if (Directory.Exists(candidate))
                return candidate;
            dir = dir.Parent;
        }

        throw new InvalidOperationException("Could not locate docs/conformance.");
    }
}
