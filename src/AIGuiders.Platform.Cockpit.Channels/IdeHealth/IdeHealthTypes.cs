#nullable enable
using AIGuiders.Platform.Cockpit.ComputingUnits;

namespace AIGuiders.Platform.Cockpit.Channels.IdeHealth;

/// <summary>Signal stratum for IDE Health (ADR 0095) — semantic level, not severity.</summary>
public enum IdeHealthStratum
{
    Workspace,
    Solution,
    Ide,
}

/// <summary>Scope within solution stratum (ADR 0095).</summary>
public enum IdeHealthScope
{
    Solution,
    Project,
}

/// <summary>Segment source in the IDE Health strip (ADR 0021).</summary>
public enum IdeHealthSource
{
    Build,
    Tests,
    Debug,
    Git,
}

/// <summary>One segment line before mapping to display (ADR 0089).</summary>
public readonly record struct IdeHealthSegmentInput(
    string LineText,
    string CockpitShort,
    bool IsBuildRunning = false,
    IdeHealthStratum Stratum = IdeHealthStratum.Solution,
    IdeHealthScope Scope = IdeHealthScope.Solution,
    string? ProjectPath = null);

public readonly record struct IdeHealthWorkspaceInput(IdeHealthSegmentInput Git);

public readonly record struct IdeHealthSolutionInput(
    IdeHealthSegmentInput Build,
    IdeHealthSegmentInput Tests,
    IdeHealthSegmentInput Debug);

public readonly record struct IdeHealthIdeHostInput(string? LspStatusHint = null);

/// <summary>CCU payload: IDE Health input snapshot (ADR 0089/0095/0097).</summary>
public readonly record struct IdeHealthInputSnapshot(
    IdeHealthWorkspaceInput Workspace,
    IdeHealthSolutionInput Solution,
    IdeHealthIdeHostInput IdeHost) : ICockpitComputeUnitPayload
{
    public static IdeHealthInputSnapshot FromFlat(
        IdeHealthSegmentInput build,
        IdeHealthSegmentInput tests,
        IdeHealthSegmentInput debug,
        IdeHealthSegmentInput git) =>
        new(new IdeHealthWorkspaceInput(git), new IdeHealthSolutionInput(build, tests, debug), default);
}

/// <summary>Channel build context for IDE Health (ADR 0036).</summary>
public readonly record struct IdeHealthChannelContext
{
    public static IdeHealthChannelContext Default => default;
}
