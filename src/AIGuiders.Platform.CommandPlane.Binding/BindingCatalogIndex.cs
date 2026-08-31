using AIGuiders.Platform.IntermediateRepresentation.Binding;
#nullable enable

using AIGuiders.Platform.Catalog;

namespace AIGuiders.Platform.CommandPlane.Binding;

public sealed class BindingCatalogIndex
{
    readonly CatalogIndex<string, BindingEntry> _index;

    BindingCatalogIndex(CatalogIndex<string, BindingEntry> index) =>
        _index = index;

    public static BindingCatalogIndex Empty { get; } = new(
        CatalogIndex<string, BindingEntry>.Empty(StringComparer.OrdinalIgnoreCase));

    public IReadOnlyCollection<BindingEntry> Entries => _index.Entries;

    public static BindingCatalogIndex FromDescriptors(IEnumerable<BindingDescriptor> descriptors) =>
        new(CatalogIndex<string, BindingEntry>.FromDescriptors(descriptors, BindingCatalogProfile.Instance));

    public BindingCatalogIndex Merge(BindingCatalogIndex overlay) =>
        new(_index.MergeOverlayWins(overlay._index));

    public bool TryGetByKey(string bindingKey, out BindingEntry entry) =>
        _index.TryGet(bindingKey, out entry!);

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
