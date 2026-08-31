#nullable enable
using AIGuiders.Platform.CommandPlane;

namespace AIGuiders.Platform.CommandPlane.Catalog.Sources;

/// <summary>XML command catalog sources (GUIDERS-ADR-0013).</summary>
public static class XmlCommandSources
{
    public static ICommandSource FromXml(string content, string? sourceId = null) =>
        CommandSource.FromText(content, XmlCommandFormatReader.Instance, sourceId ?? "xml");

    public static ICommandSource FromFile(string path, string? sourceId = null) =>
        CommandSource.FromFile(path, XmlCommandFormatReader.Instance, sourceId);
}
