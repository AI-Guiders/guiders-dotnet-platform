#nullable enable
using AIGuiders.Platform.CommandPlane;

namespace AIGuiders.Platform.CommandPlane.Sources;

/// <summary>JSON command catalog sources (GUIDERS-ADR-0013).</summary>
public static class JsonCommandSources
{
    public static ICommandSource FromJson(string content, string? sourceId = null) =>
        CommandSource.FromText(content, JsonCommandFormatReader.Instance, sourceId ?? "json");

    public static ICommandSource FromFile(string path, string? sourceId = null) =>
        CommandSource.FromFile(path, JsonCommandFormatReader.Instance, sourceId);
}
