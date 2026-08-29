#nullable enable
using System.Text.Json;
using AIGuiders.Platform.CommandPlane;

namespace AIGuiders.Platform.CommandPlane.Sources;

static class CommandDescriptorMapper
{
    public static SlashCommandDescriptor FromDictionary(IReadOnlyDictionary<string, string> fields)
    {
        var commandId = Require(fields, "commandId", "command_id");
        var path = Require(fields, "path");
        return new SlashCommandDescriptor
        {
            Domain = Get(fields, "domain") ?? "",
            Object = Get(fields, "object") ?? "",
            Intent = Get(fields, "intent") ?? "",
            CommandId = commandId,
            Path = path,
            PathAliases = ParseList(Get(fields, "pathAliases", "path_aliases")),
            Help = Get(fields, "help"),
            Group = Get(fields, "group", "slash_group"),
            ArgTail = Get(fields, "argTail", "arg_tail") ?? "optional",
            ArgHint = Get(fields, "argHint", "arg_hint"),
            ArgPickerChoices = ParsePickerChoices(Get(fields, "argPickerChoices", "arg_picker_choices")),
            Surfaces = ParseList(Get(fields, "surfaces")),
            RequiredCapabilities = ParseList(Get(fields, "requiredCapabilities", "required_capabilities")),
            Tier = Get(fields, "tier"),
            PluginId = Get(fields, "pluginId", "plugin_id"),
            RequiresDestructiveConfirm = ParseBool(Get(fields, "requiresDestructiveConfirm", "requires_destructive_confirm")),
        };
    }

    public static Dictionary<string, string> JsonToDictionary(JsonElement element)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var property in element.EnumerateObject())
        {
            dict[property.Name] = property.Value.ValueKind switch
            {
                JsonValueKind.String => property.Value.GetString() ?? "",
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Array => string.Join(',', property.Value.EnumerateArray().Select(ReadJsonScalar)),
                _ => property.Value.GetRawText(),
            };
        }

        return dict;
    }

    static string ReadJsonScalar(JsonElement element) =>
        element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? "",
            _ => element.GetRawText(),
        };

    static string Require(IReadOnlyDictionary<string, string> fields, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (fields.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        throw new InvalidOperationException($"Command descriptor is missing required field '{keys[0]}'.");
    }

    static string? Get(IReadOnlyDictionary<string, string> fields, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (fields.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return null;
    }

    static IReadOnlyList<string> ParseList(string? raw) =>
        string.IsNullOrWhiteSpace(raw)
            ? []
            : raw.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    static IReadOnlyList<SlashPickerChoice> ParsePickerChoices(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return [];
        }

        var choices = new List<SlashPickerChoice>();
        foreach (var token in raw.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = token.Split(':', 2, StringSplitOptions.TrimEntries);
            var value = parts[0];
            var label = parts.Length > 1 ? parts[1] : value;
            choices.Add(new SlashPickerChoice { Value = value, Label = label });
        }

        return choices;
    }

    static bool ParseBool(string? raw) =>
        bool.TryParse(raw, out var value) && value;
}
