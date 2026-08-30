using AIGuiders.Platform.Utilities.Adoption;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class AdoptionScannerTests
{
    [Fact]
    public void Scan_csproj_pins_command_plane_hyperlane()
    {
        var temp = Path.Combine(Path.GetTempPath(), "adoption-scan-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temp);
        try
        {
            File.WriteAllText(
                Path.Combine(temp, "Sample.csproj"),
                """
                <Project Sdk="Microsoft.NET.Sdk">
                  <ItemGroup>
                    <PackageReference Include="AIGuiders.Platform.CommandPlane.Slash" Version="0.10.0" />
                  </ItemGroup>
                </Project>
                """);

            var planet = new PlanetConfig("test", "Test Planet", ".", null);
            var rules = new[]
            {
                new HyperlaneRule("AIGuiders.Platform.CommandPlane", "CommandPlane", "dotnet-nuget"),
            };

            var rows = PlanetAdoptionScanner.ScanPlanet(planet, temp, rules);
            Assert.Single(rows);
            Assert.Equal("CommandPlane", rows[0].Hyperlane);
            Assert.Contains("CommandPlane.Slash", rows[0].Packages);
        }
        finally
        {
            Directory.Delete(temp, recursive: true);
        }
    }

    [Fact]
    public void AllianceReportWriter_emits_table_header()
    {
        var rows = new[]
        {
            new PlanetAdoptionRow("forge", "Agent Forge", "CommandPlane", "dotnet-nuget (0.4.2)",
                "AIGuiders.Platform.CommandPlane", "slash-arg-completion-v1",
                "https://github.com/AI-Guiders/agent-forge/issues"),
        };

        var md = AllianceReportWriter.ToMarkdown(rows, new DateTimeOffset(2026, 8, 30, 3, 0, 0, TimeSpan.Zero));
        Assert.Contains("| Planet | Hyperlane | Port |", md);
        Assert.Contains("Agent Forge", md);
        Assert.Contains("slash-arg-completion-v1", md);
    }
}
