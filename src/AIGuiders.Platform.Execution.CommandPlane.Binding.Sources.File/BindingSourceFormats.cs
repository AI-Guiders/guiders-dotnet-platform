using AIGuiders.Platform.IntermediateRepresentation.Binding;
#nullable enable

using AIGuiders.Platform.Execution.Sources;

namespace AIGuiders.Platform.Execution.CommandPlane.Binding.Sources;

internal static class BindingSourceFormats
{
    public static BindingDocumentFormat Resolve(string pathOrResourceName) =>
        ToBindingFormat(DocumentFormats.Resolve(pathOrResourceName));

    internal static BindingDocumentFormat ToBindingFormat(DocumentFormat format) =>
        format switch
        {
            DocumentFormat.Json => BindingDocumentFormat.Json,
            DocumentFormat.Toml => BindingDocumentFormat.Toml,
            DocumentFormat.Xml => throw new NotSupportedException(
                "Binding catalogs do not support .xml. Use .json or .toml."),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
        };
}

internal static class BindingFormatReaders
{
    public static IBindingFormatReader For(BindingDocumentFormat format) =>
        format switch
        {
            BindingDocumentFormat.Json => JsonBindingFormatReader.Instance,
            BindingDocumentFormat.Toml => TomlBindingFormatReader.Instance,
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
        };
}
