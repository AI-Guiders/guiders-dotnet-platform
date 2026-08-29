#nullable enable
using System.Text.Json;
using AIGuiders.Platform.CommandPlane;

namespace AIGuiders.Platform.CommandPlane.Sources;

public sealed class JsonCommandFormatReader : ICommandFormatReader
{
    public static JsonCommandFormatReader Instance { get; } = new();

    static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public string FormatName => "json";

    public IReadOnlyList<SlashCommandDescriptor> Read(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        using var document = JsonDocument.Parse(text);
        var root = document.RootElement;
        if (root.ValueKind == JsonValueKind.Array)
        {
            return root.EnumerateArray().Select(ParseCommand).ToList();
        }

        if (root.TryGetProperty("commands", out var commands) && commands.ValueKind == JsonValueKind.Array)
        {
            return commands.EnumerateArray().Select(ParseCommand).ToList();
        }

        throw new InvalidOperationException("JSON command document must be a commands[] array or { \"commands\": [] }.");
    }

    static SlashCommandDescriptor ParseCommand(JsonElement element) =>
        CommandDescriptorMapper.FromDictionary(CommandDescriptorMapper.JsonToDictionary(element));
}
