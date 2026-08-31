#nullable enable

namespace AIGuiders.Platform.IntermediateRepresentation.Melody;

/// <summary>Line-level capture and validation policy for a melody (GUIDERS-ADR-0015 §7).</summary>
public enum MelodyLineProfile
{
    /// <summary>Every step is <see cref="MelodyArticulation.ByNote"/> (CIDE/Glass default).</summary>
    PureByNote,

    /// <summary>Every step is <see cref="MelodyArticulation.ByChord"/>.</summary>
    PureByChord,

    /// <summary>Explicit hybrid line — steps may mix note and chord articulation.</summary>
    Mixed,
}
