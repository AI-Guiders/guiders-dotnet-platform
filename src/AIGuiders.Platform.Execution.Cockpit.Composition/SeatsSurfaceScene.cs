#nullable enable

namespace AIGuiders.Platform.Execution.Cockpit.Composition;

/// <summary>Inputs projected by peels — compositor assembles seats surface (ADR 0036).</summary>
public readonly record struct SeatsSurfaceScene(
    string SchemaVersion,
    string Mfd,
    object View,
    object Seats,
    object Session,
    object? Instrument,
    object? Alert,
    object? Pressure,
    object Next,
    object? Focus,
    object? Go,
    object? Warm,
    string?[] Pins,
    string[] Layouts,
    string? ThrashNote,
    object? Loci,
    string[]? GoVerbs);

public readonly record struct SeatsSurfacePayload(int SeatCount);
