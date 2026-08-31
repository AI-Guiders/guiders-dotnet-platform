using AIGuiders.Platform.IntermediateRepresentation.Command;
#nullable enable

namespace AIGuiders.Platform.CommandPlane;

/// <summary>Helpers for static picker descriptors (closed enumerations).</summary>
public static class CommandPickerChoices
{
    public static IReadOnlyList<CommandPickerChoice> FromValues(params string[] values) =>
        values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => new CommandPickerChoice { Value = value.Trim() })
            .ToList();

    public static IReadOnlyList<CommandPickerChoice> FromLabels(
        params (string Value, string Label)[] entries) =>
        entries
            .Where(entry => !string.IsNullOrWhiteSpace(entry.Value))
            .Select(entry => new CommandPickerChoice
            {
                Value = entry.Value.Trim(),
                Label = string.IsNullOrWhiteSpace(entry.Label) ? entry.Value.Trim() : entry.Label.Trim(),
            })
            .ToList();

    public static IReadOnlyList<CommandPickerChoice> FromEnum<TEnum>()
        where TEnum : struct, Enum =>
        Enum.GetNames<TEnum>()
            .Select(name => new CommandPickerChoice
            {
                Value = name,
                Label = name,
            })
            .ToList();
}
