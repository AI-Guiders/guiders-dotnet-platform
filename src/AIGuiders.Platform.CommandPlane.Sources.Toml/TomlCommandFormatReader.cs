#nullable enable
using AIGuiders.Platform.CommandPlane;
using AIGuiders.Platform.Notations.Argument;
using Tomlyn;
using Tomlyn.Model;

namespace AIGuiders.Platform.CommandPlane.Sources;

public sealed class TomlCommandFormatReader : ICommandFormatReader
{
    public static TomlCommandFormatReader Instance { get; } = new();

    public string FormatName => "toml";

    public IReadOnlyList<SlashCommandDescriptor> Read(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        var model = Toml.ToModel(text);
        var commands = new List<SlashCommandDescriptor>();

        foreach (var key in new[] { "commands", "command" })
        {
            if (!model.TryGetValue(key, out var node) || node is not TomlTableArray array)
            {
                continue;
            }

            commands.AddRange(array.Select(ParseTable));
            return commands;
        }

        return commands;
    }

    static SlashCommandDescriptor ParseTable(TomlTable table)
    {
        var dict = table.ToDictionary(
            pair => pair.Key,
            pair => pair.Value?.ToString() ?? "",
            StringComparer.OrdinalIgnoreCase);

        if (table.TryGetValue("path_aliases", out var aliasesNode) && aliasesNode is TomlArray aliases)
        {
            dict["pathAliases"] = string.Join(',', aliases.Select(x => x?.ToString() ?? ""));
        }

        if (table.TryGetValue("arg_picker_choices", out var choicesNode) && choicesNode is TomlArray choices)
        {
            dict["argPickerChoices"] = string.Join('|', choices.Select(x => x?.ToString() ?? ""));
        }

        var descriptor = CommandDescriptorMapper.FromDictionary(dict);
        return CommandDescriptorMapper.WithInvocationSchema(
            descriptor,
            GetTailWireClass(table),
            ParseArgParameters(table));
    }

    static string? GetTailWireClass(TomlTable table)
    {
        foreach (var key in new[] { "tail_wire_class", "tailWireClass" })
        {
            if (table.TryGetValue(key, out var node))
                return node?.ToString()?.Trim();
        }

        return null;
    }

    static IReadOnlyList<InvocationArgParameter> ParseArgParameters(TomlTable table)
    {
        if (!table.TryGetValue("arg_parameters", out var node) && !table.TryGetValue("argParameters", out node))
            return [];

        if (node is not TomlTableArray array)
            return [];

        return array.Select(ParseArgParameter).ToList();
    }

    static InvocationArgParameter ParseArgParameter(TomlTable table)
    {
        var name = ReadString(table, "name");
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("arg_parameters[] entry requires 'name'.");

        return new InvocationArgParameter(
            name.Trim(),
            ParseKind(ReadString(table, "kind")),
            ReadString(table, "long_option", "longOption"),
            ReadString(table, "short_option", "shortOption"));
    }

    static InvocationArgParameterKind ParseKind(string? raw) =>
        raw?.Trim().ToLowerInvariant() switch
        {
            "flag" => InvocationArgParameterKind.Flag,
            "positional" => InvocationArgParameterKind.Positional,
            _ => InvocationArgParameterKind.Value,
        };

    static string? ReadString(TomlTable table, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (table.TryGetValue(key, out var node))
                return node?.ToString()?.Trim();
        }

        return null;
    }
}
