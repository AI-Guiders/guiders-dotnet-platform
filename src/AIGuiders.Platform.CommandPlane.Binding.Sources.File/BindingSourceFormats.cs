#nullable enable

namespace AIGuiders.Platform.CommandPlane.Binding.Sources;

internal static class BindingSourceFormats
{
    public static BindingDocumentFormat Resolve(string pathOrResourceName) =>
        Path.GetExtension(pathOrResourceName).ToLowerInvariant() switch
        {
            ".json" => BindingDocumentFormat.Json,
            ".toml" => BindingDocumentFormat.Toml,
            _ => throw new NotSupportedException(
                $"Unsupported binding catalog extension '{Path.GetExtension(pathOrResourceName)}'. Use .json or .toml."),
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
