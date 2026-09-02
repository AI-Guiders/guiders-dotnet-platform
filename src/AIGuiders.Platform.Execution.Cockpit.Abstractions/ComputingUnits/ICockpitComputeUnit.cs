#nullable enable

namespace AIGuiders.Platform.Execution.Cockpit.ComputingUnits;

/// <summary>CCU marker (CIDE ADR 0097) — fold between transport and channel DTO.</summary>
public interface ICockpitComputeUnit
{
}

/// <summary>Payload/DTO on the CCU boundary — not a compute unit itself.</summary>
public interface ICockpitComputeUnitPayload
{
}
