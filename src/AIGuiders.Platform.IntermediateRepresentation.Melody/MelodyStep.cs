#nullable enable

namespace AIGuiders.Platform.IntermediateRepresentation.Melody;

/// <summary>One step in a melody line after chord root is engaged.</summary>
public sealed class MelodyStep
{
    public required MelodyArticulation Articulation { get; init; }

    /// <summary>Note character or chord wire (e.g. <c>b</c>, <c>Ctrl+R</c>).</summary>
    public required string Wire { get; init; }

    /// <summary>Optional tail slot parser id for parametric steps.</summary>
    public string? ReaderId { get; init; }
}
