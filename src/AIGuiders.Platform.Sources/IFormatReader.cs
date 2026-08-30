#nullable enable

namespace AIGuiders.Platform.Sources;

/// <summary>Parses a text document into a typed model (JSON, TOML, XML, …).</summary>
public interface IFormatReader<out TOut>
{
    string FormatName { get; }

    TOut Read(string text);
}
