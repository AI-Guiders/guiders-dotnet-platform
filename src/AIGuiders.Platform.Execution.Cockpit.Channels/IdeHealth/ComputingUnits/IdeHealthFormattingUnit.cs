#nullable enable

using AIGuiders.Platform.Execution.Cockpit.ComputingUnits;
using AIGuiders.Platform.Modeling.Cockpit.DataBus;

namespace AIGuiders.Platform.Execution.Cockpit.Channels.IdeHealth.ComputingUnits;

public sealed class IdeHealthFormattingUnit : ICockpitComputeUnit
{
    public static IdeHealthFormattingUnit Default { get; } = new();

    public IdeHealthSegmentInput BuildSegment(BuildStateSnapshot s)
    {
        if (s.IsBuilding)
        {
            return new IdeHealthSegmentInput(
                "Build: running…",
                "BUILD…",
                IsBuildRunning: true,
                Stratum: IdeHealthStratum.Solution,
                Scope: IdeHealthScope.Solution);
        }

        var line = s.LastBuildSucceeded.HasValue switch
        {
            true when s.LastBuildSucceeded.Value && s.LastExitCode.HasValue =>
                $"Build: idle · last OK (exit {s.LastExitCode.Value})",
            true when s.LastBuildSucceeded.Value => "Build: idle · last OK",
            false when s.LastExitCode.HasValue =>
                $"Build: idle · last failed (exit {s.LastExitCode.Value})",
            false => "Build: idle · last failed",
            _ => "Build: idle"
        };
        return new IdeHealthSegmentInput(line, "READY", Stratum: IdeHealthStratum.Solution, Scope: IdeHealthScope.Solution);
    }

    public IdeHealthSegmentInput TestsSegment(string? lastTestSummary, int impactedTestsBadge)
    {
        var line = !string.IsNullOrWhiteSpace(lastTestSummary)
            ? $"Tests: {lastTestSummary}"
            : $"Tests: impacted {impactedTestsBadge}";
        var cockpit = !string.IsNullOrWhiteSpace(lastTestSummary)
            ? (lastTestSummary.Length > 36 ? string.Concat(lastTestSummary.AsSpan(0, 33), "…") : lastTestSummary)
            : $"imp {impactedTestsBadge}";
        return new IdeHealthSegmentInput(line, cockpit, Stratum: IdeHealthStratum.Solution, Scope: IdeHealthScope.Solution);
    }

    public IdeHealthSegmentInput DebugSegment(
        bool hasActiveSession,
        bool executionStopped,
        int stackFrameCount,
        int variableCount)
    {
        if (!hasActiveSession)
            return new IdeHealthSegmentInput("Debug: idle", "DBG · —", Stratum: IdeHealthStratum.Solution, Scope: IdeHealthScope.Solution);

        if (executionStopped)
        {
            var line = $"Debug: paused · frames {stackFrameCount}, vars {variableCount}";
            return new IdeHealthSegmentInput(line, $"DBG · pause · {stackFrameCount}fr", Stratum: IdeHealthStratum.Solution, Scope: IdeHealthScope.Solution);
        }

        return new IdeHealthSegmentInput("Debug: running…", "DBG · run", Stratum: IdeHealthStratum.Solution, Scope: IdeHealthScope.Solution);
    }

    public IdeHealthSegmentInput GitSegment(string gitLine, string gitCockpitShort) =>
        new(gitLine, gitCockpitShort, Stratum: IdeHealthStratum.Workspace);

    public IdeHealthSegmentInput ProjectBuildSegment(string projectPath, BuildStateSnapshot s)
    {
        if (s.IsBuilding)
        {
            return new IdeHealthSegmentInput(
                $"Build[{projectPath}]: running…",
                "BUILD…",
                IsBuildRunning: true,
                Stratum: IdeHealthStratum.Solution,
                Scope: IdeHealthScope.Project,
                ProjectPath: projectPath);
        }

        var tail = s.LastBuildSucceeded.HasValue switch
        {
            true when s.LastBuildSucceeded.Value && s.LastExitCode.HasValue =>
                $"idle · last OK (exit {s.LastExitCode.Value})",
            true when s.LastBuildSucceeded.Value => "idle · last OK",
            false when s.LastExitCode.HasValue => $"idle · last failed (exit {s.LastExitCode.Value})",
            false => "idle · last failed",
            _ => "idle"
        };
        return new IdeHealthSegmentInput(
            $"Build[{projectPath}]: {tail}",
            "READY",
            Stratum: IdeHealthStratum.Solution,
            Scope: IdeHealthScope.Project,
            ProjectPath: projectPath);
    }

    public IdeHealthSegmentInput ProjectTestsSegment(string projectPath, string? summary, int impactedTestsBadge)
    {
        var normalizedSummary = string.IsNullOrWhiteSpace(summary)
            ? $"impacted {impactedTestsBadge}"
            : summary;
        return new IdeHealthSegmentInput(
            $"Tests[{projectPath}]: {normalizedSummary}",
            normalizedSummary.Length > 36 ? string.Concat(normalizedSummary.AsSpan(0, 33), "…") : normalizedSummary,
            Stratum: IdeHealthStratum.Solution,
            Scope: IdeHealthScope.Project,
            ProjectPath: projectPath);
    }

    public IdeHealthSegmentInput ProjectDebugSegment(string projectPath, string summary) =>
        new(
            $"Debug[{projectPath}]: {summary}",
            summary.Length > 36 ? string.Concat(summary.AsSpan(0, 33), "…") : summary,
            Stratum: IdeHealthStratum.Solution,
            Scope: IdeHealthScope.Project,
            ProjectPath: projectPath);
}
