#nullable enable

namespace AIGuiders.Platform.CommandPlane.Binding.Sources;

public static class DatabaseBindingSources
{
    public static IBindingSource From(
        Func<IReadOnlyList<BindingDescriptor>> query,
        string? sourceId = null) =>
        BindingSource.From(query, sourceId ?? "db");
}
