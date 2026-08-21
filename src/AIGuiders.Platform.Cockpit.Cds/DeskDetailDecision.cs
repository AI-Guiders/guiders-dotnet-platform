#nullable enable
using AIGuiders.Platform.Cockpit.ComputingUnits;

namespace AIGuiders.Platform.Cockpit.Cds;

/// <summary>CDS decision: desk_detail / nav_detail resolution (ADR 0097).</summary>
public readonly record struct DeskDetailDecision(
    string DeskDetail,
    bool WantNav) : ICockpitComputeUnitPayload;
