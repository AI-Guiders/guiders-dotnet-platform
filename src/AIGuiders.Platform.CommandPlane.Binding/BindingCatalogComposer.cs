#nullable enable

namespace AIGuiders.Platform.CommandPlane.Binding;

public static class BindingCatalogComposer
{
    public static BindingCatalogIndex Build(params IBindingSource[] sources) =>
        Build((IEnumerable<IBindingSource>)sources);

    public static BindingCatalogIndex Build(IEnumerable<IBindingSource> sources)
    {
        var merged = new Dictionary<string, BindingDescriptor>(StringComparer.OrdinalIgnoreCase);
        foreach (var source in sources)
        {
            foreach (var descriptor in source.Load())
                merged[descriptor.BindingKey] = descriptor;
        }

        return BindingCatalogIndex.FromDescriptors(merged.Values);
    }
}
