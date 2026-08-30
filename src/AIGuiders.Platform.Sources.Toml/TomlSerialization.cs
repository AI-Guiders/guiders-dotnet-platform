#nullable enable

using Tomlyn;
using System.Text.Json;

namespace AIGuiders.Platform.Sources;

public static class TomlSerialization
{
    public static TomlSerializerOptions SnakeCaseLower { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    public static T? Deserialize<T>(string text, TomlSerializerOptions? options = null) where T : class =>
        TomlSerializer.Deserialize<T>(text, options ?? SnakeCaseLower);
}
