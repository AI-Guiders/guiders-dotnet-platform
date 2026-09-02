using AIGuiders.Platform.IntermediateRepresentation.Command;
#nullable enable

namespace AIGuiders.Platform.Execution.CommandPlane;

/// <summary>Parses a text document into slash command descriptors (JSON, TOML, XML, …).</summary>
public interface ICommandFormatReader
{
    string FormatName { get; }

    IReadOnlyList<CommandDescriptor> Read(string text);
}
