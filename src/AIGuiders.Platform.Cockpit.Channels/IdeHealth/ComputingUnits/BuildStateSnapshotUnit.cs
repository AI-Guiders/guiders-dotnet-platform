#nullable enable

using AIGuiders.Platform.Cockpit.DataBus;

namespace AIGuiders.Platform.Cockpit.Channels.IdeHealth.ComputingUnits;

public static class BuildStateSnapshotUnit
{
    public static BuildStateSnapshot Apply(BuildStateSnapshot prior, BuildStateChanged e)
    {
        if (e.IsBuilding)
            return prior with { IsBuilding = true };

        if (e.LastExitCode is not null || e.LastBuildSucceeded is not null)
        {
            return new BuildStateSnapshot(
                false,
                e.LastExitCode ?? prior.LastExitCode,
                e.LastBuildSucceeded ?? prior.LastBuildSucceeded);
        }

        return prior with { IsBuilding = false };
    }
}
