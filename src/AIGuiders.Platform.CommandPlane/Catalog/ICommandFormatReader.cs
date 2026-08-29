#nullable enable

namespace AIGuiders.Platform.CommandPlane;

/// <summary>Parses a text document into slash command descriptors (JSON, TOML, XML, …).</summary>
public interface ICommandFormatReader
{
    string FormatName { get; }

    IReadOnlyList<SlashCommandDescriptor> Read(string text);
}
