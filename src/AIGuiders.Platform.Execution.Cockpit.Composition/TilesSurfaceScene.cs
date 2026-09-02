#nullable enable

namespace AIGuiders.Platform.Execution.Cockpit.Composition;

/// <summary>Legacy tiles-mode desk surface inputs (prefer seats).</summary>
public readonly record struct TilesSurfaceScene(
    string SchemaVersion,
    string Mfd,
    object Session,
    object? Tiles,
    object? Alert,
    object Next,
    object? Focus,
    object? Go,
    object? Warm,
    string[] Pins,
    string[] Layouts,
    object? Loci,
    string[]? GoVerbs);

public readonly record struct TilesSurfacePayload(int PinCount);
