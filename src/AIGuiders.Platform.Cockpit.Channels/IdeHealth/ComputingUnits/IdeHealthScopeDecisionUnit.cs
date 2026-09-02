#nullable enable

using AIGuiders.Platform.Cockpit.ComputingUnits;

namespace AIGuiders.Platform.Cockpit.Channels.IdeHealth.ComputingUnits;

public sealed class IdeHealthScopeDecisionUnit : ICockpitComputeUnit
{
    public static IdeHealthScopeDecisionUnit Default { get; } = new();

    public IdeHealthScopeDecision Decide(string? startupProjectPath, bool isBuilding, string? lastTestSummary)
    {
        var hasStartupProject = !string.IsNullOrWhiteSpace(startupProjectPath);
        var hasProjectSignal = isBuilding || !string.IsNullOrWhiteSpace(lastTestSummary);
        return hasStartupProject && hasProjectSignal
            ? new IdeHealthScopeDecision(IdeHealthScope.Project, startupProjectPath!)
            : new IdeHealthScopeDecision(IdeHealthScope.Solution, null);
    }
}
