#nullable enable
using System.Text.Json;
using System.Text.Json.Serialization;
using AIGuiders.Platform.Conformance.Schemas;
using Tomlyn;
using Tomlyn.Model;

namespace AIGuiders.Platform.Conformance.Policies;

public static class PolicySpecFormats
{
    public static IReadOnlyList<string> ValidateFile(string path)
    {
        var json = Path.GetExtension(path).Equals(".toml", StringComparison.OrdinalIgnoreCase)
            ? TomlToPolicyJson(File.ReadAllText(path))
            : File.ReadAllText(path);

        return ConformanceSchemaValidator.ValidatePolicyJson(json);
    }

    public static string TomlToPolicyJson(string toml)
    {
        var table = TomlSerializer.Deserialize<TomlTable>(toml)
            ?? throw new InvalidOperationException("Policy spec TOML deserialized to null.");
        return JsonSerializer.Serialize(ConvertTable(table), PolicySpecLoader.JsonOptions);
    }

    static Dictionary<string, object?> ConvertTable(TomlTable table)
    {
        var map = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var (key, value) in table)
            map[key] = ConvertNode(value);
        return map;
    }

    static object? ConvertNode(object? node) => node switch
    {
        TomlTable table => ConvertTable(table),
        TomlTableArray array => array.Select(ConvertNode).ToList(),
        _ => node,
    };
}
