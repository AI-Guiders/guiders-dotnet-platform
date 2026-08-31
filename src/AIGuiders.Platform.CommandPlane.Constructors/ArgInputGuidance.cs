#nullable enable

namespace AIGuiders.Platform.CommandPlane;

/// <summary>Surface-neutral line guidance after engage (GUIDERS-ADR-0043).</summary>
public sealed record ArgInputGuidance(
    InvocationLinePhase Phase,
    ArgMechanic? ArgMechanic,
    string Placeholder,
    string Hint,
    string? CanonicalPath = null,
    string ArgTailKind = "",
    string? ReadyWire = null,
    string? DisplayTail = null);
