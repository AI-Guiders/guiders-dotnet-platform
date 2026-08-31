using AIGuiders.Platform.IntermediateRepresentation.Binding;
#nullable enable

namespace AIGuiders.Platform.CommandPlane.Binding;

public static class BindingSource
{
    public static IBindingSource From(
        IEnumerable<BindingDescriptor> descriptors,
        string? sourceId = null) =>
        new DescriptorBindingSource(
            sourceId ?? "descriptors",
            descriptors as IReadOnlyList<BindingDescriptor> ?? descriptors.ToList());

    public static IBindingSource From(
        Func<IReadOnlyList<BindingDescriptor>> loader,
        string? sourceId = null) =>
        new DelegateBindingSource(sourceId ?? "delegate", loader);

    public static IBindingSource FromText(
        string text,
        IBindingFormatReader reader,
        string? sourceId = null) =>
        new TextBindingSource(sourceId ?? reader.FormatName, text, reader);

    public static IBindingSource FromFile(
        string path,
        IBindingFormatReader reader,
        string? sourceId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return FromText(File.ReadAllText(path), reader, sourceId ?? $"file:{Path.GetFileName(path)}");
    }

    sealed class DescriptorBindingSource(string sourceId, IReadOnlyList<BindingDescriptor> descriptors) : IBindingSource
    {
        public string SourceId { get; } = sourceId;

        public IReadOnlyList<BindingDescriptor> Load() => descriptors;
    }

    sealed class DelegateBindingSource(string sourceId, Func<IReadOnlyList<BindingDescriptor>> loader) : IBindingSource
    {
        public string SourceId { get; } = sourceId;

        public IReadOnlyList<BindingDescriptor> Load() => loader();
    }

    sealed class TextBindingSource(string sourceId, string text, IBindingFormatReader reader) : IBindingSource
    {
        public string SourceId { get; } = sourceId;

        public IReadOnlyList<BindingDescriptor> Load() => reader.Read(text);
    }
}
