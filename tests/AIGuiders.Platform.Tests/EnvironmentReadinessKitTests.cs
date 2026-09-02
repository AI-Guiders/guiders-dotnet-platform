#nullable enable
using AIGuiders.Platform.Execution.Cockpit.Channels.EnvironmentReadiness;
using AIGuiders.Platform.Execution.Cockpit.Channels.EnvironmentReadiness.DataAcquisition;
using AIGuiders.Platform.Execution.Cockpit.Channels.EnvironmentReadiness.ComputingUnits;
using AIGuiders.Platform.Execution.Cockpit.DataBus;
using AIGuiders.Platform.Modeling.Cockpit.DataBus;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class EnvironmentReadinessKitTests
{
    [Fact]
    public void PathAcquisition_classifies_agent_notes_file()
    {
        Assert.Equal(
            AgentNotesFilePathKind.Unset,
            EnvironmentReadinessPathAcquisition.ClassifyAgentNotesFilePath(null));
    }

    [Fact]
    public async Task SnapshotUnit_builds_core_sections()
    {
        var ctx = new EnvironmentReadinessChannelContext(
            new EnvironmentReadinessSettings(null),
            null,
            IdeHostStateChanged.Empty,
            IsMcpStdioHost: true);

        var snapshot = await EnvironmentReadinessSnapshotUnit.BuildCoreAsync(
            new EnvironmentReadinessSnapshotUnit.Input(
                ctx,
                new EnvironmentReadinessCSharpProbeOptions(),
                "unset hint"),
            CancellationToken.None);

        Assert.True(snapshot.Rows.Count >= 6);
        Assert.Contains(snapshot.Rows, r => r.Id == EnvironmentReadinessCellIds.Agent);
    }
}

public sealed class DataBusAsyncPolicyTests
{
    [Fact]
    public void Default_policy_marks_debug_burst()
    {
        Assert.True(DispatchPolicyModule.isBurstForTypeName(nameof(DebugStateChanged), DispatchPolicyModule.defaultPolicy));
        Assert.False(DispatchPolicyModule.isBurstForTypeName(nameof(BuildStateChanged), DispatchPolicyModule.defaultPolicy));
    }

    [Fact]
    public async Task Async_bus_delivers_reliable_events()
    {
        using var bus = new InMemoryDataBus(asynchronousDispatch: true);
        var tcs = new TaskCompletionSource<BuildStateChanged>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var sub = bus.Subscribe<BuildStateChanged>(e => tcs.TrySetResult(e));
        bus.Publish(new BuildStateChanged { IsBuilding = true });
        var evt = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.True(evt.IsBuilding);
    }
}
