#nullable enable

namespace AIGuiders.Platform.Catalog;

/// <summary>Maps domain descriptors to keyed catalog entries (GUIDERS-ADR-0041).</summary>
public interface ICatalogProfile<TDescriptor, TKey, TEntry>
    where TKey : notnull
{
    IEqualityComparer<TKey> KeyComparer { get; }

    CatalogIndexCollisionPolicy LayerCollisionPolicy { get; }

    CatalogIndexCollisionPolicy MergeCollisionPolicy { get; }

    IEnumerable<(TKey Key, TEntry Entry)> Project(TDescriptor descriptor);

    TKey NormalizeKey(TKey key);
}
