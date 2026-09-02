#nullable enable

using AIGuiders.Platform.Cockpit.ComputingUnits;
using AIGuiders.Platform.Cockpit.DataBus.Debug;

namespace AIGuiders.Platform.Cockpit.Channels.IdeHealth.ComputingUnits;

public sealed class IdeHealthDebugSegmentUnit : ICockpitComputeUnit
{
    public static IdeHealthDebugSegmentUnit Default { get; } = new();

    public IdeHealthSegmentInput Compose(IdeHealthScopeDecision scopeDecision, in DebugSessionSnapshot snapshot)
    {
        if (scopeDecision.Scope == IdeHealthScope.Project && !string.IsNullOrWhiteSpace(scopeDecision.ProjectPath))
        {
            var summary = IdeHealthDebugSummaryUnit.Default.Summarize(snapshot);
            return IdeHealthFormattingUnit.Default.ProjectDebugSegment(scopeDecision.ProjectPath, summary);
        }

        var variableCount = snapshot.VariableRootScopes.Sum(scope => scope.Roots.Count);
        return IdeHealthFormattingUnit.Default.DebugSegment(
            snapshot.HasActiveSession,
            snapshot.IsExecutionStopped,
            snapshot.StackFrames.Count,
            variableCount);
    }
}
