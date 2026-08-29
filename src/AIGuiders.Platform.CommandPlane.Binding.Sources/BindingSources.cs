#nullable enable

namespace AIGuiders.Platform.CommandPlane.Binding.Sources;

public static class BindingSources
{
    public static IBindingSource From(
        string content,
        BindingDocumentFormat format,
        string? sourceId = null) =>
        FileBindingSources.From(content, format, sourceId);

    public static IBindingSource FromJson(string content, string? sourceId = null) =>
        JsonBindingSources.FromJson(content, sourceId);

    public static IBindingSource FromToml(string content, string? sourceId = null) =>
        TomlBindingSources.FromToml(content, sourceId);

    public static IBindingSource FromDb(
        Func<IReadOnlyList<BindingDescriptor>> query,
        string? sourceId = null) =>
        DatabaseBindingSources.From(query, sourceId);

    public static IBindingSource FromFile(string path, string? sourceId = null) =>
        FileBindingSources.FromFile(path, sourceId);
}
