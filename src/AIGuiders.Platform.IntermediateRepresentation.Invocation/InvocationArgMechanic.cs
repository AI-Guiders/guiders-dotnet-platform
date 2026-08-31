#nullable enable

namespace AIGuiders.Platform.IntermediateRepresentation.Invocation;

/// <summary>
/// Arg-tail mechanic active during <see cref="InvocationLinePhase.Arg"/> (GUIDERS-ADR-0043).
/// Implemented by CommandPlane guilds (Constructors, PrefixArmed, ArgSuggestions, …).
/// </summary>
public enum InvocationArgMechanic
{
    Picker = 1,
    FreeText = 2,
    Optional = 3,
    Constructor = 4,
    TypedInput = 5,
}
