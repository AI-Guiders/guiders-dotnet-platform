#nullable enable

namespace AIGuiders.Platform.Catalog;

/// <summary>How duplicate keys are resolved when building or merging catalog layers.</summary>
public enum CatalogIndexCollisionPolicy
{
    /// <summary>Keep the first entry for a key (ship baseline wins).</summary>
    ShipFirst,

    /// <summary>Later entry overwrites the key (overlay wins).</summary>
    OverlayWins,
}
