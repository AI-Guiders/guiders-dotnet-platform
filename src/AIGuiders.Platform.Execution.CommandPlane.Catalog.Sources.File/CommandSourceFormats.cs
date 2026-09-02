using AIGuiders.Platform.IntermediateRepresentation.Command;
#nullable enable

using AIGuiders.Platform.Execution.Sources;

namespace AIGuiders.Platform.Execution.CommandPlane.Catalog.Sources;

internal static class CommandSourceFormats
{
    public static CommandDocumentFormat Resolve(string pathOrResourceName) =>
        ToCommandFormat(DocumentFormats.Resolve(pathOrResourceName));

    internal static CommandDocumentFormat ToCommandFormat(DocumentFormat format) =>
        format switch
        {
            DocumentFormat.Json => CommandDocumentFormat.Json,
            DocumentFormat.Toml => CommandDocumentFormat.Toml,
            DocumentFormat.Xml => CommandDocumentFormat.Xml,
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
        };
}
