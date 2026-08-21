#nullable enable

namespace AIGuiders.Platform.Cockpit.DataBus;

/// <summary>Tests pipeline state for CCU projections (ADR 0099 quarry).</summary>
public readonly record struct TestsStateChanged(string Summary, int ImpactedBadge);
