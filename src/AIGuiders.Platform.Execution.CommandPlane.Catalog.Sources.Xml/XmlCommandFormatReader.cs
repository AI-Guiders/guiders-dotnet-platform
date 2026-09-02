using AIGuiders.Platform.IntermediateRepresentation.Command;
#nullable enable
using System.Xml.Linq;
using AIGuiders.Platform.Execution.CommandPlane;

namespace AIGuiders.Platform.Execution.CommandPlane.Catalog.Sources;

public sealed class XmlCommandFormatReader : ICommandFormatReader
{
    public static XmlCommandFormatReader Instance { get; } = new();

    public string FormatName => "xml";

    public IReadOnlyList<CommandDescriptor> Read(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        var root = XDocument.Parse(text).Root
                   ?? throw new InvalidOperationException("XML command document requires a root element.");

        var commandElements = root.Name.LocalName.Equals("commands", StringComparison.OrdinalIgnoreCase)
            ? root.Elements().Where(e => e.Name.LocalName.Equals("command", StringComparison.OrdinalIgnoreCase))
            : root.Elements("command");

        return commandElements.Select(ParseCommand).ToList();
    }

    static CommandDescriptor ParseCommand(XElement element)
    {
        var attrs = element.Attributes().ToDictionary(
            a => a.Name.LocalName,
            a => a.Value,
            StringComparer.OrdinalIgnoreCase);

        foreach (var child in element.Elements())
        {
            attrs.TryAdd(child.Name.LocalName, child.Value);
        }

        return CommandDescriptorMapper.FromDictionary(attrs);
    }
}
