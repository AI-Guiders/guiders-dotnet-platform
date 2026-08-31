#nullable enable

namespace AIGuiders.Platform.IntermediateRepresentation.Command;

/// <summary>Arg tail policy after canonical slash path (CIDE ADR-0150).</summary>
public enum CommandArgTailKind
{
    None = 0,
    Optional = 1,
    Required = 2,
    /// <summary>Forge extension: picker:id — clients without picker UI degrade to Optional.</summary>
    Picker = 3,
    /// <summary>Implicit editor selection span (FORGE-ADR-0064).</summary>
    ImplicitSelection = 4,
    /// <summary>Implicit or tail-parsed 1-based line range (CIDE ADR-0081).</summary>
    ImplicitLineRange = 5,
}
