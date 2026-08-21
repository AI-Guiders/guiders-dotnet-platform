#nullable enable
using AIGuiders.Platform.Cockpit.ComputingUnits;

namespace AIGuiders.Platform.Cockpit.Cds;

/// <summary>CDS input: normalize MFD/go attention before channel dispatch (ADR 0036/0097).</summary>
public readonly record struct AttentionRoutingInput(
    string? MfdExplicit,
    string? GoVerb,
    bool SeatsMode,
    string? DefaultMfd);

/// <summary>CDS decision after attention routing.</summary>
public readonly record struct AttentionRoutingDecision(
    string Mfd,
    string? GoVerb,
    bool DeskDetailNavForced) : ICockpitComputeUnitPayload;
