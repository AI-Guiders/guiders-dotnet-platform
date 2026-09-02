#nullable enable

using AIGuiders.Platform.Execution.Cockpit.Channels.IdeHealth;
using AIGuiders.Platform.Execution.Cockpit.Channels.IdeHealth.ComputingUnits;
using AIGuiders.Platform.Execution.Cockpit.DataBus;
using AIGuiders.Platform.Modeling.Cockpit.DataBus;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class IdeHealthCcuTests
{
    [Fact]
    public void BuildStateFold_applies_exit()
    {
        var prior = BuildStateSnapshot.Empty;
        var next = BuildStateFold.apply(
            prior,
            new BuildStateChanged
            {
                IsBuilding = false,
                LastExitCode = 0,
                LastBuildSucceeded = true,
            });
        Assert.False(next.IsBuilding);
        Assert.Equal(0, next.LastExitCode);
        Assert.True(next.LastBuildSucceeded);
    }

    [Fact]
    public void SnapshotUnit_subscribes_and_builds()
    {
        using var bus = new InMemoryDataBus();
        using var unit = new IdeHealthSnapshotUnit(bus);

        bus.Publish(new BuildStateChanged { IsBuilding = true });
        bus.Publish(new TestsStateChanged { Summary = "3 passed", ImpactedBadge = 0 });
        bus.Publish(new GitStateChanged { Line = "Git: main · clean", CockpitShort = "main" });
        bus.Publish(new DebugStateChanged { Snapshot = DebugSessionSnapshot.Empty });
        bus.Publish(new IdeHostStateChanged
        {
            CSharpLspProcessActive = true,
            MarkdownLspProcessActive = false,
            CSharpLspHostPresent = true,
            MarkdownLspHostPresent = false,
        });
        bus.Publish(new StartupProjectPathChanged { ProjectPath = "App.csproj" });

        var input = unit.Build(IdeHealthChannelContext.Default);
        var output = IdeHealthOutputComposer.Compose(input);

        Assert.Contains("running", input.Solution.Build.LineText, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(4, output.Segments.Count);
        Assert.Equal(IdeHealthSource.Git, output.Segments[3].Source);
        Assert.Equal("LSP · C#", input.IdeHost.LspStatusHint);
    }

    [Fact]
    public void ScopeDecision_project_when_startup_and_building()
    {
        var decision = IdeHealthScopeDecisionUnit.Default.Decide("App.csproj", isBuilding: true, lastTestSummary: null);
        Assert.Equal(IdeHealthScope.Project, decision.Scope);
        Assert.Equal("App.csproj", decision.ProjectPath);
    }
}
