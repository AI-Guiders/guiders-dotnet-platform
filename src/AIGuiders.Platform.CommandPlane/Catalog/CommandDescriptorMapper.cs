#nullable enable
using System.Text.Json;
using AIGuiders.Platform.Notations;

namespace AIGuiders.Platform.CommandPlane;

/// <summary>Maps flat command document fields to <see cref="SlashCommandDescriptor"/> (ADR-0013).</summary>
public static class CommandDescriptorMapper
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
            TailWireClass = Get(fields, "tailWireClass", "tail_wire_class"),
            ArgHint = Get(fields, "argHint", "arg_hint"),
            ArgPickerChoices = ParsePickerChoices(Get(fields, "argPickerChoices", "arg_picker_choices")),
            Surfaces = ParseList(Get(fields, "surfaces")),
            RequiredCapabilities = ParseList(Get(fields, "requiredCapabilities", "required_capabilities")),
            Tier = Get(fields, "tier"),
            PluginId = Get(fields, "pluginId", "plugin_id"),
            RequiresDestructiveConfirm = ParseBool(Get(fields, "requiresDestructiveConfirm", "requires_destructive_confirm")),
        };
    }

    public static SlashCommandDescriptor WithInvocationSchema(
        SlashCommandDescriptor descriptor,
        string? tailWireClass,
        IReadOnlyList<InvocationArgParameter>? argParameters)
    {
        if (tailWireClass is null && (argParameters is null || argParameters.Count == 0))
            return descriptor;

        return new SlashCommandDescriptor
        {
            Domain = descriptor.Domain,
            Object = descriptor.Object,
            Intent = descriptor.Intent,
            CommandId = descriptor.CommandId,
            Path = descriptor.Path,
            PathAliases = descriptor.PathAliases,
            Help = descriptor.Help,
            Group = descriptor.Group,
            ArgTail = descriptor.ArgTail,
            TailWireClass = tailWireClass ?? descriptor.TailWireClass,
            ArgParameters = argParameters is { Count: > 0 } ? argParameters : descriptor.ArgParameters,
            ArgHint = descriptor.ArgHint,
            ArgPickerChoices = descriptor.ArgPickerChoices,
            Surfaces = descriptor.Surfaces,
            RequiredCapabilities = descriptor.RequiredCapabilities,
            Tier = descriptor.Tier,
            PluginId = descriptor.PluginId,
            RequiresDestructiveConfirm = descriptor.RequiresDestructiveConfirm,
        };
    }

    public static IReadOnlyList<InvocationArgParameter> ParseArgParametersFromJson(JsonElement element)
    {
        if (!TryGetProperty(element, "argParameters", "arg_parameters", out var parameters)
            || parameters.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return parameters.EnumerateArray().Select(ParseArgParameterJson).ToList();
    }

    public static string? GetTailWireClassFromJson(JsonElement element) =>
        TryGetProperty(element, "tailWireClass", "tail_wire_class", out var wireClass) && wireClass.ValueKind == JsonValueKind.String
            ? wireClass.GetString()?.Trim()
            : null;

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

    static InvocationArgParameter ParseArgParameterJson(JsonElement element)
    {
        var name = element.TryGetProperty("name", out var nameNode) ? nameNode.GetString() : null;
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("argParameters[] entry requires 'name'.");

        return new InvocationArgParameter(
            name.Trim(),
            ParseArgParameterKind(ReadJsonString(element, "kind")),
            ReadJsonString(element, "longOption", "long_option"),
            ReadJsonString(element, "shortOption", "short_option"));
    }

    static InvocationArgParameterKind ParseArgParameterKind(string? raw) =>
        raw?.Trim().ToLowerInvariant() switch
        {
            "flag" => InvocationArgParameterKind.Flag,
            "positional" => InvocationArgParameterKind.Positional,
            _ => InvocationArgParameterKind.Value,
        };

    static string? ReadJsonString(JsonElement element, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (element.TryGetProperty(key, out var node) && node.ValueKind == JsonValueKind.String)
                return node.GetString()?.Trim();
        }

        return null;
    }

    static bool TryGetProperty(JsonElement element, string camelKey, string snakeKey, out JsonElement value)
    {
        if (element.TryGetProperty(camelKey, out value))
            return true;

        return element.TryGetProperty(snakeKey, out value);
    }
}
