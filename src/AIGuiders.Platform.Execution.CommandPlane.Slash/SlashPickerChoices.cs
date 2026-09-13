using AIGuiders.Platform.IntermediateRepresentation.Command;
#nullable enable

namespace AIGuiders.Platform.Execution.CommandPlane;

/// <summary>Helpers for static picker descriptors (closed enumerations).</summary>
public static class CommandPickerChoices
{
    public static IReadOnlyList<CommandPickerChoice> FromValues(params string[] values) =>
        values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => CommandPickerChoiceInterop.FromValue(value.Trim()))
            .ToList();

    public static IReadOnlyList<CommandPickerChoice> FromLabels(
        params (string Value, string Label)[] entries) =>
        entries
            .Where(entry => !string.IsNullOrWhiteSpace(entry.Value))
            .Select(entry => CommandPickerChoiceInterop.FromValue(
                entry.Value.Trim(),
                string.IsNullOrWhiteSpace(entry.Label) ? entry.Value.Trim() : entry.Label.Trim()))
            .ToList();

    public static IReadOnlyList<CommandPickerChoice> FromEnum<TEnum>()
        where TEnum : struct, Enum =>
        Enum.GetNames<TEnum>()
            .Select(name => CommandPickerChoiceInterop.FromValue(name, name))
            .ToList();
}
