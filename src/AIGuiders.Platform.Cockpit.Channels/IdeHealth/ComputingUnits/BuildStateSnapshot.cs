#nullable enable

namespace AIGuiders.Platform.Cockpit.Channels.IdeHealth.ComputingUnits;

/// <summary>Build fold state for IdeHealth CCU (ADR 0097).</summary>
public readonly record struct BuildStateSnapshot(
    bool IsBuilding,
    int? LastExitCode = null,
    bool? LastBuildSucceeded = null)
{
    public static BuildStateSnapshot Empty { get; } = new(false, null, null);
}
