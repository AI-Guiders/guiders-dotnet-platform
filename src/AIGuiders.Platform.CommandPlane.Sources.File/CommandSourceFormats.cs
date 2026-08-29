#nullable enable

namespace AIGuiders.Platform.CommandPlane.Sources;

internal static class CommandSourceFormats
{
    public static CommandDocumentFormat Resolve(string pathOrResourceName) =>
        Path.GetExtension(pathOrResourceName).ToLowerInvariant() switch
        {
            ".json" => CommandDocumentFormat.Json,
            ".toml" => CommandDocumentFormat.Toml,
            ".xml" => CommandDocumentFormat.Xml,
            _ => throw new NotSupportedException(
                $"Unsupported command catalog extension '{Path.GetExtension(pathOrResourceName)}'. Use .json, .toml, or .xml."),
        };
}
