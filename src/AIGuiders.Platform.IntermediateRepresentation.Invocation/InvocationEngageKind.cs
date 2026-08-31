#nullable enable

namespace AIGuiders.Platform.IntermediateRepresentation.Invocation;

/// <summary>
/// How invocation started after engage consume (GUIDERS-ADR-0015) — notation-bound.
/// Distinct from <see cref="ArgMechanic"/> (arg-tail interaction, notation-agnostic).
/// </summary>
public enum InvocationEngageKind
{
    Slash = 1,
    Melody = 2,
    Binding = 3,
}
