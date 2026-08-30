#nullable enable
using AIGuiders.Platform.Configurations.Workspace;
using AIGuiders.Platform.Documentation.Correspondence;
using AIGuiders.Platform.Sources;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class WorkspaceSourcesTests
{
    [Fact]
    public void FromText_parses_snake_case_workspace_sections()
    {
        const string toml = """
            [workspace.adr]
            root_dir = "docs/adr"
            max_related = 5

            [[workspace.features.feature]]
            id = "core"
            title = "Core"
            paths = ["src/**"]
            docs = ["README.md"]
            """;

        var doc = WorkspaceSources.FromText(toml, "fixture").Load();

        Assert.Equal("fixture", WorkspaceSources.FromText(toml, "fixture").SourceId);
        Assert.Equal("docs/adr", doc.Workspace!.Adr!.RootDir);
        Assert.Equal(5, doc.Workspace.Adr.MaxRelated);
        Assert.Single(doc.Workspace.Features!.Feature);
        Assert.Equal("core", doc.Workspace.Features.Feature[0].Id);
    }

    [Fact]
    public void DocumentFormats_matches_command_and_binding_dispatch()
    {
        Assert.Equal(DocumentFormat.Json, DocumentFormats.Resolve("catalog.json"));
        Assert.Equal(DocumentFormat.Toml, DocumentFormats.Resolve("hotkeys.toml"));
        Assert.Throws<NotSupportedException>(() => DocumentFormats.Resolve("notes.txt"));
    }

    [Fact]
    public void TryLoadCascade_and_TryResolve_round_trip()
    {
        var root = Path.Combine(Path.GetTempPath(), "guiders-ws-" + Guid.NewGuid().ToString("N"));
        var cascadeDir = Path.Combine(root, ".cascade");
        Directory.CreateDirectory(cascadeDir);

        try
        {
            const string toml = """
                [[workspace.features.feature]]
                id = "demo"
                paths = ["src/Demo.cs"]
                docs = ["docs/demo.md"]
                """;

            File.WriteAllText(Path.Combine(cascadeDir, "workspace.toml"), toml);

            var doc = WorkspaceSources.TryLoadCascade(root);
            Assert.NotNull(doc);
            Assert.Equal("demo", doc!.Workspace!.Features!.Feature[0].Id);

            var demoPath = Path.Combine(root, "src", "Demo.cs");
            Directory.CreateDirectory(Path.GetDirectoryName(demoPath)!);
            File.WriteAllText(demoPath, "// demo");

            var result = CorrespondenceResolver.TryResolve(demoPath, root);
            Assert.NotNull(result);
            Assert.Equal("src/Demo.cs", result!.FileRel);
            Assert.Contains("L1p", result.ActiveLayers);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
