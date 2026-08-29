#nullable enable

namespace AIGuiders.Platform.CommandPlane.Binding;

public sealed class BindingCatalogIndex
{
    readonly Dictionary<string, BindingEntry> _byKey;

    BindingCatalogIndex(Dictionary<string, BindingEntry> byKey) =>
        _byKey = byKey;

    public static BindingCatalogIndex Empty { get; } = new(new(StringComparer.OrdinalIgnoreCase));

    public IReadOnlyCollection<BindingEntry> Entries => _byKey.Values;

    public static BindingCatalogIndex FromDescriptors(IEnumerable<BindingDescriptor> descriptors)
    {
        var byKey = new Dictionary<string, BindingEntry>(StringComparer.OrdinalIgnoreCase);
        foreach (var descriptor in descriptors)
        {
            BindingGestureNormalizer.TryNormalizeWire(descriptor.GestureWire, out var normalized, out _);
            byKey[descriptor.BindingKey] = new BindingEntry(descriptor, normalized);
        }

        return new BindingCatalogIndex(byKey);
    }

    public BindingCatalogIndex Merge(BindingCatalogIndex overlay)
    {
        var merged = new Dictionary<string, BindingEntry>(_byKey, StringComparer.OrdinalIgnoreCase);
        foreach (var (key, entry) in overlay._byKey)
            merged[key] = entry;

        return new BindingCatalogIndex(merged);
    }

    public bool TryGetByKey(string bindingKey, out BindingEntry entry) =>
        _byKey.TryGetValue(bindingKey, out entry!);

    public bool TryGetDisplayHint(string commandId, out string gestureWire)
    {
        gestureWire = "";
        if (!TryGetByKey(commandId, out var entry))
            return false;

        if (entry.Descriptor.TargetKind != BindingTargetKind.Command)
            return false;

        gestureWire = entry.Descriptor.GestureWire;
        return true;
    }

    public bool TryGetChordRoot(out BindingEntry entry)
    {
        if (TryGetByKey(BindingWellKnownKeys.CascadeChord, out entry!))
            return true;

        entry = null!;
        return false;
    }
}
