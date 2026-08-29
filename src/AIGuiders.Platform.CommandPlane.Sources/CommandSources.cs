#nullable enable
using AIGuiders.Platform.CommandPlane;

namespace AIGuiders.Platform.CommandPlane.Sources;

/// <summary>Format-specific <see cref="CommandSource"/> factories (JSON, TOML, XML, DB, file).</summary>
public static class CommandSources
{
    public static ICommandSource From(
        string content,
        CommandDocumentFormat format,
        string? sourceId = null) =>
        CommandSource.FromText(content, CommandFormatReaders.For(format), sourceId ?? format.ToString().ToLowerInvariant());

    public static ICommandSource FromJson(string content, string? sourceId = null) =>
        CommandSource.FromText(content, CommandFormatReaders.Json, sourceId ?? "json");

    public static ICommandSource FromToml(string content, string? sourceId = null) =>
        CommandSource.FromText(content, CommandFormatReaders.Toml, sourceId ?? "toml");

    public static ICommandSource FromXml(string content, string? sourceId = null) =>
        CommandSource.FromText(content, CommandFormatReaders.Xml, sourceId ?? "xml");

    public static ICommandSource FromDb(
        Func<IReadOnlyList<SlashCommandDescriptor>> query,
        string? sourceId = null) =>
        CommandSource.From(query, sourceId ?? "db");

    public static ICommandSource FromFile(string path, string? sourceId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var format = ResolveFormat(path);
        var content = File.ReadAllText(path);
        return From(content, format, sourceId ?? $"file:{Path.GetFileName(path)}");
    }

    static CommandDocumentFormat ResolveFormat(string path) =>
        Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".json" => CommandDocumentFormat.Json,
            ".toml" => CommandDocumentFormat.Toml,
            ".xml" => CommandDocumentFormat.Xml,
            _ => throw new NotSupportedException(
                $"Unsupported command catalog file extension '{Path.GetExtension(path)}'. Use .json, .toml, or .xml."),
        };
}
