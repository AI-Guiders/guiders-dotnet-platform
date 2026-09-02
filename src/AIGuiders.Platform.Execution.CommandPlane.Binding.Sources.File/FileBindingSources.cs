using AIGuiders.Platform.IntermediateRepresentation.Binding;
#nullable enable

namespace AIGuiders.Platform.Execution.CommandPlane.Binding.Sources;

public static class FileBindingSources
{
    public static IBindingSource FromFile(string path, string? sourceId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var format = BindingSourceFormats.Resolve(path);
        return BindingSource.FromFile(path, BindingFormatReaders.For(format), sourceId ?? $"file:{Path.GetFileName(path)}");
    }

    public static IBindingSource From(
        string content,
        BindingDocumentFormat format,
        string? sourceId = null) =>
        BindingSource.FromText(content, BindingFormatReaders.For(format), sourceId ?? format.ToString().ToLowerInvariant());
}
