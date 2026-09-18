#nullable enable

using AIGuiders.Platform.Execution.Ide.Session;
using AIGuiders.Platform.Modeling.Core.Identity;
using AIGuiders.Platform.Modeling.Ide.Session;
using AIGuiders.Platform.Modeling.LanguageIntelligence.Relations;
using Microsoft.FSharp.Core;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class FederationSessionRuntimeBuildHookTests
{
    static string FindPlatformSlnx()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            var candidate = Path.Combine(dir.FullName, "AIGuiders.Platform.slnx");
            if (File.Exists(candidate))
                return candidate;
        }

        throw new InvalidOperationException("Could not locate AIGuiders.Platform.slnx for federation runtime hook test.");
    }

    [Fact]
    public void TryIngestBuildDiagnostics_updates_cached_session_after_open()
    {
        var slnx = FindPlatformSlnx();
        var opened = FederationSessionRuntime.Open(slnx);
        Assert.True(opened.IsValid);

        var firstPath = opened.Runtime.Registry.Values.First().Path.Value;
        var result = FederationSessionRuntime.TryIngestBuildDiagnostics(
            slnx,
            [
                new BuildDiagnosticIngest.BuildDiagnosticWire(
                    firstPath,
                    1,
                    1,
                    "CS0246",
                    "type not found"),
            ]);

        Assert.NotNull(result);
        Assert.Single(result!.Diagnostics);
        Assert.Single(result.Runtime.Diagnostics);

        var found = DiagnosticIndexOps.tryFindByCode("CS0246", result.Runtime.Diagnostics);
        Assert.True(FSharpOption<Tuple<Identity<Diagnostic, NumericId>, DiagnosticRecord>>.get_IsSome(found));
    }

    [Fact]
    public void TryRunBuildAndIngestDiagnostics_ingests_compile_errors_from_dotnet_build()
    {
        var root = Path.Combine(Path.GetTempPath(), "federation-build-" + Guid.NewGuid().ToString("N"));
        var appDir = Path.Combine(root, "App");
        Directory.CreateDirectory(appDir);

        try
        {
            File.WriteAllText(
                Path.Combine(appDir, "App.csproj"),
                """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <TargetFramework>net10.0</TargetFramework>
                  </PropertyGroup>
                </Project>
                """);

            File.WriteAllText(
                Path.Combine(appDir, "Broken.cs"),
                """
                namespace FederationBuildTest;

                public sealed class Broken
                {
                    public InvalidType MissingField;
                }
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

            var result = FederationSessionRuntime.TryRunBuildAndIngestDiagnostics(slnx);
            Assert.NotNull(result);
            Assert.NotEmpty(result!.Diagnostics);
            Assert.NotEmpty(result.Runtime.Diagnostics);

            var found = DiagnosticIndexOps.tryFindByCode("CS0246", result.Runtime.Diagnostics);
            Assert.True(FSharpOption<Tuple<Identity<Diagnostic, NumericId>, DiagnosticRecord>>.get_IsSome(found));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }
}
