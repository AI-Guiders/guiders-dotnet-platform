#nullable enable

using AIGuiders.Platform.Cockpit.ComputingUnits;

namespace AIGuiders.Platform.Cockpit.Channels.IdeHealth.ComputingUnits;

public readonly record struct IdeHealthBuildTestsSnapshot(
    IdeHealthSegmentInput Build,
    IdeHealthSegmentInput Tests);

public sealed class IdeHealthBuildTestsUnit : ICockpitComputeUnit
{
    public static IdeHealthBuildTestsUnit Default { get; } = new();

    public IdeHealthBuildTestsSnapshot Compose(
        IdeHealthScopeDecision scopeDecision,
        BuildStateSnapshot buildState,
        string? testSummary,
        int impactedTestsBadge)
    {
        if (scopeDecision.Scope == IdeHealthScope.Project && !string.IsNullOrWhiteSpace(scopeDecision.ProjectPath))
        {
            var projectPath = scopeDecision.ProjectPath;
            return new IdeHealthBuildTestsSnapshot(
                IdeHealthFormattingUnit.Default.ProjectBuildSegment(projectPath, buildState),
                IdeHealthFormattingUnit.Default.ProjectTestsSegment(projectPath, testSummary, impactedTestsBadge));
        }

        return new IdeHealthBuildTestsSnapshot(
            IdeHealthFormattingUnit.Default.BuildSegment(buildState),
            IdeHealthFormattingUnit.Default.TestsSegment(testSummary, impactedTestsBadge));
    }
}
