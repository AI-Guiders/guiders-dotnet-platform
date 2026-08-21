#nullable enable

using AIGuiders.Platform.Cockpit.DataBus.Debug;

namespace AIGuiders.Platform.Cockpit.DataBus;

/// <summary>Debug session snapshot for IdeHealth and other projections (ADR 0099 quarry).</summary>
public readonly record struct DebugStateChanged(DebugSessionSnapshot Snapshot);
