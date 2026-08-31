#nullable enable

namespace AIGuiders.Platform.IntermediateRepresentation.Invocation;

/// <summary>
/// Where the user is on the invocation line after engage (GUIDERS-ADR-0043).
/// Orthogonal to engage (Slash / Melody / Binding per ADR-0015).
/// </summary>
public enum InvocationLinePhase
{
    /// <summary>Completing command path or melody slug steps.</summary>
    Path = 0,

    /// <summary>Collecting arg tail — <see cref="InvocationArgMechanic"/> applies here.</summary>
    Arg = 1,

    /// <summary>Line is runnable — Enter executes.</summary>
    Ready = 2,
}
