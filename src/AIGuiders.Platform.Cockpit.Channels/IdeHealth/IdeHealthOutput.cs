#nullable enable

namespace AIGuiders.Platform.Cockpit.Channels.IdeHealth;

/// <summary>One segment after output fold (ADR 0036 compositor input).</summary>
public sealed class IdeHealthSegment
{
    public required IdeHealthSource Source { get; init; }
    public required IdeHealthStratum Stratum { get; init; }
    public required IdeHealthScope Scope { get; init; }
    public string? ProjectPath { get; init; }
    public required string LineText { get; init; }
    public required string CockpitShort { get; init; }
    public bool IsBuildRunning { get; init; }
    public bool IsBuildSource => Source == IdeHealthSource.Build;
}

/// <summary>Channel output snapshot for MCP/Glass bind (ADR 0089 quarry).</summary>
public sealed class IdeHealthOutputSnapshot
{
    public required IReadOnlyList<IdeHealthSegment> Segments { get; init; }
    public IdeHealthIdeHostInput IdeHost { get; init; }
}
