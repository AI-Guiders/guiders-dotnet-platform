#nullable enable

namespace AIGuiders.Platform.Combinations;

/// <summary>Documented merge semantics for platform combinators (GUIDERS-ADR-0030).</summary>
public enum CombinationSemantics
{
    /// <summary>Field-level overlay: non-null overlay fields win; baseline fills gaps.</summary>
    FieldOverlay,

    /// <summary>Whole-section replace when overlay section is present.</summary>
    SectionReplace,

    /// <summary>Baseline wins on key collision (ship catalog + user additions).</summary>
    ShipFirst,

    /// <summary>Overlay wins on key collision (user hotkey overrides).</summary>
    OverlayWins,
}
