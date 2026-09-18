#nullable enable

using AIGuiders.Platform.Execution.Documentation.Correspondence;
using AIGuiders.Platform.Execution.Ide.Session;
using AIGuiders.Platform.Modeling.Ide.Session;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class FederationSessionRuntimeCorrespondenceHookTests
{
    [Fact]
    public void Open_ingests_crs_doc_to_code_witnesses_when_workspace_root_exists()
    {
        var root = Path.Combine(Path.GetTempPath(), "federation-crs-" + Guid.NewGuid().ToString("N"));
        var cascadeDir = Path.Combine(root, ".cascade");
        var srcDir = Path.Combine(root, "src");
        var docsDir = Path.Combine(root, "docs", "adr");
        Directory.CreateDirectory(cascadeDir);
        Directory.CreateDirectory(srcDir);
        Directory.CreateDirectory(docsDir);

        try
        {
            File.WriteAllText(
                Path.Combine(cascadeDir, "workspace.toml"),
                """
                [[workspace.correspondence.code_anchors]]
                doc = "docs/adr/test.md"
                file = "src/Foo.cs"
                kind = "normates"
                member_key = "Bar"
                """);

            File.WriteAllText(Path.Combine(docsDir, "test.md"), "# ADR\n\nSee src/Foo.cs Bar.");
            File.WriteAllText(Path.Combine(srcDir, "Foo.cs"), "class Bar {}");

            var csprojDir = Path.Combine(root, "App");
            Directory.CreateDirectory(csprojDir);
            File.WriteAllText(
                Path.Combine(csprojDir, "App.csproj"),
                """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <TargetFramework>net10.0</TargetFramework>
                  </PropertyGroup>
                  <ItemGroup>
                    <Compile Include="../src/Foo.cs" Link="Foo.cs" />
                  </ItemGroup>
                </Project>
                """);

            var slnx = Path.Combine(root, "App.slnx");
            File.WriteAllText(
                slnx,
                """
                <Solution>
                  <Project Path="App/App.csproj" />
                </Solution>
                """);

            var opened = FederationSessionRuntime.Open(slnx);
            Assert.True(opened.IsValid);
            Assert.Contains(
                opened.Runtime.Session.Graph.Relations,
                relation => relation.Type == RelationType.Normates);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void CorrespondenceKind_NormalizeWire_maps_canon_tokens()
    {
        Assert.Equal(CorrespondenceKind.Normates, CorrespondenceKind.NormalizeWire("normates"));
        Assert.Equal(CorrespondenceKind.ImplementsObligation, CorrespondenceKind.NormalizeWire("implements"));
        Assert.Equal(CorrespondenceKind.Documents, CorrespondenceKind.NormalizeWire(null));
        Assert.Equal(CorrespondenceKind.Documents, CorrespondenceKind.NormalizeWire("not-a-kind"));
    }
}
