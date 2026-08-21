#nullable enable

namespace AIGuiders.Platform.Cockpit.DataBus;

/// <summary>Build pipeline state for CCU projections (ADR 0099 quarry).</summary>
public readonly record struct BuildStateChanged(
    bool IsBuilding,
    int? LastExitCode = null,
    bool? LastBuildSucceeded = null);
