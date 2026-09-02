#nullable enable

using AIGuiders.Platform.Modeling.Catalog;

namespace AIGuiders.Platform.Execution.CommandPlane.Binding;

/// <summary>Binding catalog profile: binding_key, overlay-wins merge (GUIDERS-ADR-0041).</summary>
public sealed class BindingCatalogProfile : ICatalogProfile<BindingDescriptor, string, BindingEntry>
{
    public static BindingCatalogProfile Instance { get; } = new();

    public IEqualityComparer<string> KeyComparer => StringComparer.OrdinalIgnoreCase;

    public CatalogIndexCollisionPolicy LayerCollisionPolicy => CatalogIndexCollisionPolicy.OverlayWins;

    public CatalogIndexCollisionPolicy MergeCollisionPolicy => CatalogIndexCollisionPolicy.OverlayWins;

    public IEnumerable<(string, BindingEntry)> Project(BindingDescriptor descriptor)
    {
        BindingGestureNormalizer.TryNormalizeWire(descriptor.GestureWire, out var normalized, out _);
        yield return (descriptor.BindingKey, new BindingEntry(descriptor, normalized));
    }

    public string NormalizeKey(string key) => key;
}
