using AIGuiders.Platform.IntermediateRepresentation.Command;
#nullable enable
using AIGuiders.Platform.CommandPlane;

namespace AIGuiders.Platform.CommandPlane.Catalog.Sources;

/// <summary>Command catalog source facades — meta-bundle re-exports (GUIDERS-ADR-0013).</summary>
public static class CommandSources
{
    public static ICommandSource From(
        string content,
        CommandDocumentFormat format,
        string? sourceId = null) =>
        FileCommandSources.From(content, format, sourceId);

    public static ICommandSource FromJson(string content, string? sourceId = null) =>
        JsonCommandSources.FromJson(content, sourceId);

    public static ICommandSource FromToml(string content, string? sourceId = null) =>
        TomlCommandSources.FromToml(content, sourceId);

    public static ICommandSource FromXml(string content, string? sourceId = null) =>
        XmlCommandSources.FromXml(content, sourceId);

    public static ICommandSource FromDb(
        Func<IReadOnlyList<CommandDescriptor>> query,
        string? sourceId = null) =>
        DatabaseCommandSources.From(query, sourceId);

    public static ICommandSource FromFile(string path, string? sourceId = null) =>
        FileCommandSources.FromFile(path, sourceId);
}
