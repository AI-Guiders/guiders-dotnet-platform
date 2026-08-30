#nullable enable
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIGuiders.Platform.Conformance.Policies;

public static class PolicySpecLoader
{
    public static PolicySpecDocument LoadJson(string json) =>
        JsonSerializer.Deserialize<PolicySpecDocument>(json, JsonOptions)
        ?? throw new InvalidOperationException("Policy spec JSON deserialized to null.");

    public static PolicySpecDocument LoadToml(string toml) =>
        LoadJson(PolicySpecFormats.TomlToPolicyJson(toml));

    public static PolicySpecDocument LoadFile(string path)
    {
        var text = File.ReadAllText(path);
        return Path.GetExtension(path).Equals(".toml", StringComparison.OrdinalIgnoreCase)
            ? LoadToml(text)
            : LoadJson(text);
    }

    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower,
        Converters = { new JsonStringEnumConverter() },
    };
}
