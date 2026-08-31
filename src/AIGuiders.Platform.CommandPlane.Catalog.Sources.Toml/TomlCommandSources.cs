#nullable enable
using AIGuiders.Platform.CommandPlane;

namespace AIGuiders.Platform.CommandPlane.Catalog.Sources;

/// <summary>TOML command catalog sources (GUIDERS-ADR-0013).</summary>
public static class TomlCommandSources
{
    public static ICommandSource FromToml(string content, string? sourceId = null) =>
        CommandSource.FromText(content, TomlCommandFormatReader.Instance, sourceId ?? "toml");

    public static ICommandSource FromFile(string path, string? sourceId = null) =>
        CommandSource.FromFile(path, TomlCommandFormatReader.Instance, sourceId);
}
