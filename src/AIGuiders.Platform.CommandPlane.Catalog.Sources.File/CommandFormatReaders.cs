using AIGuiders.Platform.IntermediateRepresentation.Command;
#nullable enable
using AIGuiders.Platform.CommandPlane;

namespace AIGuiders.Platform.CommandPlane.Catalog.Sources;

/// <summary>Built-in format readers for <see cref="CommandSource.FromText"/>.</summary>
public static class CommandFormatReaders
{
    public static ICommandFormatReader Json { get; } = JsonCommandFormatReader.Instance;

    public static ICommandFormatReader Toml { get; } = TomlCommandFormatReader.Instance;

    public static ICommandFormatReader Xml { get; } = XmlCommandFormatReader.Instance;

    public static ICommandFormatReader For(CommandDocumentFormat format) =>
        format switch
        {
            CommandDocumentFormat.Json => Json,
            CommandDocumentFormat.Toml => Toml,
            CommandDocumentFormat.Xml => Xml,
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
        };
}
