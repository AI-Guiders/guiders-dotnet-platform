#nullable enable

namespace AIGuiders.Platform.Catalog;

/// <summary>Builds a catalog index from layered descriptor sources via a profile.</summary>
public static class CatalogLayerComposer
{
    public static CatalogIndex<TKey, TEntry> Compose<TDescriptor, TKey, TEntry>(
        ICatalogProfile<TDescriptor, TKey, TEntry> profile,
        IEnumerable<IEnumerable<TDescriptor>> layers)
        where TKey : notnull
    {
        var index = CatalogIndex<TKey, TEntry>.Empty(profile.KeyComparer);
        foreach (var layer in layers)
        {
            var next = CatalogIndex<TKey, TEntry>.FromDescriptors(layer, profile);
            index = index.Merge(next, profile.MergeCollisionPolicy);
        }

        return index;
    }
}
