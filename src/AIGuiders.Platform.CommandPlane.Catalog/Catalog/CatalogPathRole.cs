#nullable enable

namespace AIGuiders.Platform.CommandPlane;

/// <summary>Canonical vs alias path (CIDE ADR-0154 elision).</summary>
public enum CatalogPathRole
{
    Canonical,
    Alias,
}
