#nullable enable

namespace AIGuiders.Platform.Catalog;

/// <summary>Profile-driven keyed catalog index.</summary>
public sealed class CatalogIndex<TKey, TEntry>
    where TKey : notnull
{
    readonly Dictionary<TKey, TEntry> _byKey;
    readonly IEqualityComparer<TKey> _comparer;

    CatalogIndex(Dictionary<TKey, TEntry> byKey, IEqualityComparer<TKey> comparer)
    {
        _byKey = byKey;
        _comparer = comparer;
    }

    public static CatalogIndex<TKey, TEntry> Empty(IEqualityComparer<TKey> comparer) =>
        new(new Dictionary<TKey, TEntry>(comparer), comparer);

    public static CatalogIndex<TKey, TEntry> FromMap(
        IDictionary<TKey, TEntry> entries,
        IEqualityComparer<TKey> comparer) =>
        new(new Dictionary<TKey, TEntry>(entries, comparer), comparer);

    public IReadOnlyCollection<TEntry> Entries => _byKey.Values;

    public IEnumerable<TKey> Keys => _byKey.Keys;

    public bool TryGet(TKey key, out TEntry entry) =>
        _byKey.TryGetValue(key, out entry!);

    public static CatalogIndex<TKey, TEntry> FromDescriptors<TDescriptor>(
        IEnumerable<TDescriptor> descriptors,
        ICatalogProfile<TDescriptor, TKey, TEntry> profile)
    {
        var byKey = new Dictionary<TKey, TEntry>(profile.KeyComparer);
        foreach (var descriptor in descriptors)
        {
            foreach (var (key, entry) in profile.Project(descriptor))
            {
                var normalized = profile.NormalizeKey(key);
                ApplyCollision(byKey, normalized, entry, profile.LayerCollisionPolicy);
            }
        }

        return new CatalogIndex<TKey, TEntry>(byKey, profile.KeyComparer);
    }

    public CatalogIndex<TKey, TEntry> Merge(
        CatalogIndex<TKey, TEntry> overlay,
        CatalogIndexCollisionPolicy policy)
    {
        var merged = new Dictionary<TKey, TEntry>(_byKey, _comparer);
        foreach (var (key, entry) in overlay._byKey)
            ApplyCollision(merged, key, entry, policy);

        return new CatalogIndex<TKey, TEntry>(merged, _comparer);
    }

    public CatalogIndex<TKey, TEntry> MergeShipFirst(CatalogIndex<TKey, TEntry> overlay) =>
        Merge(overlay, CatalogIndexCollisionPolicy.ShipFirst);

    public CatalogIndex<TKey, TEntry> MergeOverlayWins(CatalogIndex<TKey, TEntry> overlay) =>
        Merge(overlay, CatalogIndexCollisionPolicy.OverlayWins);

    static void ApplyCollision(
        Dictionary<TKey, TEntry> byKey,
        TKey key,
        TEntry entry,
        CatalogIndexCollisionPolicy policy)
    {
        switch (policy)
        {
            case CatalogIndexCollisionPolicy.ShipFirst:
                byKey.TryAdd(key, entry);
                break;
            case CatalogIndexCollisionPolicy.OverlayWins:
                byKey[key] = entry;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(policy), policy, null);
        }
    }
}
