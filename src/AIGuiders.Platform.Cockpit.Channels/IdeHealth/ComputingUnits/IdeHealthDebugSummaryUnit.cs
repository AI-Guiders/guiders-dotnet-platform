#nullable enable

using AIGuiders.Platform.Cockpit.ComputingUnits;
using AIGuiders.Platform.Cockpit.DataBus.Debug;

namespace AIGuiders.Platform.Cockpit.Channels.IdeHealth.ComputingUnits;

public sealed class IdeHealthDebugSummaryUnit : ICockpitComputeUnit
{
    public static IdeHealthDebugSummaryUnit Default { get; } = new();

    public string Summarize(in DebugSessionSnapshot snapshot)
    {
        if (!snapshot.HasActiveSession)
            return "idle";

        if (!snapshot.IsExecutionStopped)
            return "running…";

        var variableCount = snapshot.VariableRootScopes.Sum(scope => scope.Roots.Count);
        return $"paused · frames {snapshot.StackFrames.Count}, vars {variableCount}";
    }
}
