using AIGuiders.Platform.IntermediateRepresentation.Command;
#nullable enable
using AIGuiders.Platform.Execution.CommandPlane;
using AIGuiders.Platform.IntermediateRepresentation.Argument;
using Tomlyn;
using Tomlyn.Model;

namespace AIGuiders.Platform.Execution.CommandPlane.Catalog.Sources;

public sealed class TomlCommandFormatReader : ICommandFormatReader
{
    public static TomlCommandFormatReader Instance { get; } = new();

    public string FormatName => "toml";

    public IReadOnlyList<CommandDescriptor> Read(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        var model = TomlSerializer.Deserialize<TomlTable>(text)!;
        var commands = new List<CommandDescriptor>();

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

    static CommandDescriptor ParseTable(TomlTable table)
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
        return CommandDescriptorMapper.WithArgumentNotation(descriptor, ParseArgumentNotation(table));
    }

    static ArgumentNotationProfile? ParseArgumentNotation(TomlTable table)
    {
        var readerId = GetTailWireClass(table);
        var slots = ParseArgumentSlots(table);
        if (readerId is null && slots.Count == 0)
            return null;

        return new ArgumentNotationProfile(readerId, slots.Count > 0 ? slots : null);
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

    static IReadOnlyList<ArgumentSlot> ParseArgumentSlots(TomlTable table)
    {
        if (!table.TryGetValue("arg_parameters", out var node) && !table.TryGetValue("argParameters", out node))
            return [];

        if (node is not TomlTableArray array)
            return [];

        return array.Select(ParseArgumentSlot).ToList();
    }

    static ArgumentSlot ParseArgumentSlot(TomlTable table)
    {
        var name = ReadString(table, "name");
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("arg_parameters[] entry requires 'name'.");

        return new ArgumentSlot(
            name.Trim(),
            ParseKind(ReadString(table, "kind")),
            ReadString(table, "long_option", "longOption"),
            ReadString(table, "short_option", "shortOption"));
    }

    static ArgumentSlotKind ParseKind(string? raw) =>
        raw?.Trim().ToLowerInvariant() switch
        {
            "flag" => ArgumentSlotKind.Flag,
            "positional" => ArgumentSlotKind.Positional,
            _ => ArgumentSlotKind.Value,
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
