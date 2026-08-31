#nullable enable

namespace AIGuiders.Platform.IntermediateRepresentation.Invocation;

/// <summary>
/// Arg-tail interaction during <see cref="InvocationLinePhase.Arg"/> (GUIDERS-ADR-0043).
/// Notation-agnostic — distinct from <see cref="InvocationEngageKind"/>.
/// Implemented by CommandPlane guilds (Constructors, PrefixArmed, ArgSuggestions, …).
/// </summary>
public enum ArgMechanic
{
    Picker = 1,
    FreeText = 2,
    Optional = 3,
    Constructor = 4,
    TypedInput = 5,
}
