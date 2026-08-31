#nullable enable

namespace AIGuiders.Platform.IntermediateRepresentation.Command;

/// <summary>Canonical vs alias path (CIDE ADR-0154 elision).</summary>
public enum CatalogPathRole
{
    Canonical,
    Alias,
}
