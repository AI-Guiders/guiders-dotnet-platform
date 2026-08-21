#nullable enable

namespace AIGuiders.Platform.Cockpit.DataBus;

/// <summary>Published after a cockpit compositor completes a desk surface (ADR 0099).</summary>
public readonly record struct DeskSurfaceBuiltEvent(
    string Mode,
    int SeatCount,
    string? Go,
    DateTimeOffset Utc);
