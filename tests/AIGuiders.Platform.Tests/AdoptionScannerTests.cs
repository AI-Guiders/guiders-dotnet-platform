using AIGuiders.Platform.Utilities.Adoption;
using AIGuiders.Platform.Utilities.Adoption.Reports.Markdown;
using AIGuiders.Platform.Utilities.Adoption.Sources;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class AdoptionScannerTests
{
    [Fact]
    public void CsProj_reader_emits_command_plane_pin()
    {
        var temp = Path.Combine(Path.GetTempPath(), "adoption-scan-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temp);
        try
        {
            var csproj = Path.Combine(temp, "Sample.csproj");
            File.WriteAllText(
                csproj,
                """
                <Project Sdk="Microsoft.NET.Sdk">
                  <ItemGroup>
                    <PackageReference Include="AIGuiders.Platform.CommandPlane.Slash" Version="0.10.0" />
                  </ItemGroup>
                </Project>
                """);

            var partial = CsProjAdoptionReader.ReadFile(csproj, "AIGuiders.Platform.");
            Assert.Single(partial.Pins);
            Assert.Equal("AIGuiders.Platform.CommandPlane.Slash", partial.Pins[0].PackageId);
            Assert.Equal(AdoptionPortKind.NuGetPin, partial.Pins[0].PortKind);
        }
        finally
        {
            Directory.Delete(temp, recursive: true);
        }
    }

    [Fact]
    public void Source_IR_Report_pipeline_produces_alliance_row()
    {
        var temp = Path.Combine(Path.GetTempPath(), "adoption-pipeline-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temp);
        try
        {
            File.WriteAllText(
                Path.Combine(temp, "App.csproj"),
                """
                <Project Sdk="Microsoft.NET.Sdk">
                  <ItemGroup>
                    <PackageReference Include="AIGuiders.Platform.CommandPlane" Version="0.4.2" />
                  </ItemGroup>
                </Project>
                """);

            var planet = new PlanetConfig("test", "Test Planet", ".", "https://example.test/issues");
            var rules = new[] { new HyperlaneRule("AIGuiders.Platform.CommandPlane", "CommandPlane", "dotnet-nuget") };
            var rows = AdoptionAllianceBuilder.BuildPlanet(
                planet,
                temp,
                rules,
                [AdoptionSources.FromPlanetTree()]);

            Assert.Single(rows);
            Assert.Equal("CommandPlane", rows[0].Hyperlane);
        }
        finally
        {
            Directory.Delete(temp, recursive: true);
        }
    }

    [Fact]
    public void Markdown_report_writer_emits_table_header()
    {
        var rows = new[]
        {
            new PlanetAdoptionRow("forge", "Agent Forge", "CommandPlane", "dotnet-nuget (0.4.2)",
                "AIGuiders.Platform.CommandPlane", "slash-arg-completion",
                "https://github.com/AI-Guiders/agent-forge/issues"),
        };

        var md = new MarkdownAllianceReportWriter().Write(rows, new DateTimeOffset(2026, 8, 30, 3, 0, 0, TimeSpan.Zero));
        Assert.Contains("| Planet | Hyperlane | Port |", md);
        Assert.Contains("Agent Forge", md);
        Assert.Contains("slash-arg-completion", md);
    }
}
