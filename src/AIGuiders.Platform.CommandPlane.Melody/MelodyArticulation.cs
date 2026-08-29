#nullable enable

namespace AIGuiders.Platform.CommandPlane.Melody;

/// <summary>How one step in a melody line is played (GUIDERS-ADR-0015 §7).</summary>
public enum MelodyArticulation
{
    /// <summary>Single key after chord root (e.g. <c>b</c>).</summary>
    ByNote,

    /// <summary>Simultaneous modifier+key as one step (e.g. <c>Ctrl+R</c>).</summary>
    ByChord,
}
