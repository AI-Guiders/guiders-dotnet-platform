#nullable enable
using AIGuiders.Platform.CommandPlane;
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

        return CommandDescriptorMapper.FromDictionary(dict);
    }
}
