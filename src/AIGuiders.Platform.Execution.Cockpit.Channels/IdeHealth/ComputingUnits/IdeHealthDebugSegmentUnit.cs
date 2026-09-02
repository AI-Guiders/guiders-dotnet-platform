#nullable enable

using AIGuiders.Platform.Execution.Cockpit.ComputingUnits;
using AIGuiders.Platform.Modeling.Cockpit.DataBus;

namespace AIGuiders.Platform.Execution.Cockpit.Channels.IdeHealth.ComputingUnits;

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

        var variableCount = snapshot.VariableRootScopes.Sum(scope => scope.Roots.Length);
        return IdeHealthFormattingUnit.Default.DebugSegment(
            snapshot.HasActiveSession,
            snapshot.IsExecutionStopped,
            snapshot.StackFrames.Length,
            variableCount);
    }
}
