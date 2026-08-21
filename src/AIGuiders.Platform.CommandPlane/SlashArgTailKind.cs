#nullable enable

namespace AIGuiders.Platform.CommandPlane;

/// <summary>Arg tail policy after canonical slash path (CIDE ADR-0150).</summary>
public enum SlashArgTailKind
{
    None = 0,
    Optional = 1,
    Required = 2,
    /// <summary>Forge extension: picker:id — clients without picker UI degrade to Optional.</summary>
    Picker = 3,
}
