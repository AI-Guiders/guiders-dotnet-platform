#nullable enable

using Tomlyn;

namespace AIGuiders.Platform.Execution.Sources;

public sealed class TomlFormatReader<T> : IFormatReader<T> where T : class
{
    public TomlFormatReader(string? formatName = null, TomlSerializerOptions? options = null)
    {
        FormatName = formatName ?? "toml";
        _options = options;
    }

    public string FormatName { get; }

    readonly TomlSerializerOptions? _options;

    public T Read(string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        return TomlSerialization.Deserialize<T>(text, _options)
            ?? throw new InvalidOperationException("TOML document deserialized to null.");
    }

    public static TomlFormatReader<T> SnakeCase { get; } = new(options: TomlSerialization.SnakeCaseLower);
}
