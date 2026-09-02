#nullable enable

namespace AIGuiders.Platform.Execution.Sources;

public static class DocumentFormats
{
    public static DocumentFormat Resolve(string pathOrResourceName) =>
        Path.GetExtension(pathOrResourceName).ToLowerInvariant() switch
        {
            ".json" => DocumentFormat.Json,
            ".toml" => DocumentFormat.Toml,
            ".xml" => DocumentFormat.Xml,
            _ => throw new NotSupportedException(
                $"Unsupported document extension '{Path.GetExtension(pathOrResourceName)}'. Use .json, .toml, or .xml."),
        };
}
