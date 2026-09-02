#nullable enable

namespace AIGuiders.Platform.Cockpit.Channels.IdeHealth.ComputingUnits;

public static class IdeHealthStrataComposer
{
    public static IdeHealthInputSnapshot Compose(
        IdeHealthWorkspaceInput workspace,
        IdeHealthSolutionInput solution,
        IdeHealthIdeHostInput ideHost = default) =>
        new(workspace, solution, ideHost);
}
