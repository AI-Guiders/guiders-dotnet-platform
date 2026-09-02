#nullable enable
using AIGuiders.Platform.Execution.Configurations.Project;
using AIGuiders.Platform.Execution.Configurations.Workspace;
using AIGuiders.Platform.Execution.Language.Xml.Anchors;
using AIGuiders.Platform.Execution.LanguageIntelligence.Anchors;
using AIGuiders.Platform.Navigation;
using AIGuiders.Platform.Navigation.Code;
using AIGuiders.Platform.Navigation.Policy;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class NavigationSceneJsonTests
{
    [Fact]
    public void ToJson_emits_navigation_scene_v1_schema()
    {
        const string wire = """
            {
              "mode": "related",
              "anchor_path": "src/Widget.cs",
              "items": [{ "path": "src/Widget.Part.cs", "kind": "partial_peer" }]
            }
            """;

        var scene = NavigationCodeExplorer.ExploreRelatedFromWire(wire, NavigationProfile.ExploreDefault);
        var json = NavigationSceneJson.ToJson(scene);

        Assert.Contains("\"schema\":\"navigation_scene/v1\"", json, StringComparison.Ordinal);
        Assert.Contains("\"mode\":\"related\"", json, StringComparison.Ordinal);
        Assert.Equal(NavigationSchemes.SceneV1, scene.Schema);
    }
}

public sealed class WorkspaceExploreCorrPolicyTests
{
    [Fact]
    public void Longest_path_rule_wins()
    {
        var settings = new WorkspaceExploreCorrSettings
        {
            Default = "full",
            Rules =
            [
                new WorkspaceExploreCorrRule { Path = "knowledge/", Mode = "card" },
                new WorkspaceExploreCorrRule { Path = "knowledge/work/", Mode = "off" },
            ],
        };

        var mode = WorkspaceExploreCorrPolicy.ResolveMode(
            @"C:\ws\knowledge\work\projects\card.md",
            @"C:\ws",
            settings);

        Assert.Equal(WorkspaceExploreCorrPolicy.Mode.Off, mode);
    }
}

public sealed class ProjectSourcesTests
{
    [Fact]
    public void Merge_overlay_prefers_disk_test_framework()
    {
        var baseline = new ProjectDocument
        {
            Test = new ProjectTestSettings { Policy = "detect", Framework = "xunit" },
        };
        var overlay = new ProjectDocument
        {
            Test = new ProjectTestSettings { Framework = "nunit" },
        };

        var merged = ProjectSources.MergeDocuments(baseline, overlay);

        Assert.Equal("nunit", merged.Test?.Framework);
        Assert.Equal("detect", merged.Test?.Policy);
    }
}

public sealed class XmlBracketAnchorResolveTests
{
    [Fact]
    public void Resolves_element_text_range()
    {
        const string xml = """
            <Root>
              <Item>hello</Item>
            </Root>
            """;
        var span = BracketAnchorWire.Parse("[F:doc.xml;X:Root/Item]");

        var ok = XmlBracketAnchorResolve.TryResolve(
            "doc.xml",
            xml,
            span,
            out var result,
            out var detail);

        Assert.True(ok, detail);
        Assert.False(result.Insert);
        Assert.Equal("xml_text", result.Detail);
        Assert.True(result.Range.LineStart >= 1);
    }
}
