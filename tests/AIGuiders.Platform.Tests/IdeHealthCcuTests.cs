#nullable enable

using AIGuiders.Platform.Cockpit.Channels.IdeHealth;
using AIGuiders.Platform.Cockpit.Channels.IdeHealth.ComputingUnits;
using AIGuiders.Platform.Cockpit.DataBus;
using AIGuiders.Platform.Cockpit.DataBus.Debug;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class IdeHealthCcuTests
{
    [Fact]
    public void BuildStateSnapshotUnit_applies_exit()
    {
        var prior = new BuildStateSnapshot(false, null, null);
        var next = BuildStateSnapshotUnit.Apply(prior, new BuildStateChanged(false, 0, true));
        Assert.False(next.IsBuilding);
        Assert.Equal(0, next.LastExitCode);
        Assert.True(next.LastBuildSucceeded);
    }

    [Fact]
    public void SnapshotUnit_subscribes_and_builds()
    {
        using var bus = new InMemoryDataBus();
        using var unit = new IdeHealthSnapshotUnit(bus);

        bus.Publish(new BuildStateChanged(true));
        bus.Publish(new TestsStateChanged("3 passed", 0));
        bus.Publish(new GitStateChanged("Git: main · clean", "main"));
        bus.Publish(new DebugStateChanged(DebugSessionSnapshot.Empty));
        bus.Publish(new IdeHostStateChanged(true, false, true, false));
        bus.Publish(new StartupProjectPathChanged("App.csproj"));

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
