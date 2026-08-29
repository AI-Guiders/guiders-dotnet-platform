#nullable enable

namespace AIGuiders.Platform.CommandPlane;

/// <summary>Helpers for static picker descriptors (closed enumerations).</summary>
public static class SlashPickerChoices
{
    public static IReadOnlyList<SlashPickerChoice> FromValues(params string[] values) =>
        values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => new SlashPickerChoice { Value = value.Trim() })
            .ToList();

    public static IReadOnlyList<SlashPickerChoice> FromLabels(
        params (string Value, string Label)[] entries) =>
        entries
            .Where(entry => !string.IsNullOrWhiteSpace(entry.Value))
            .Select(entry => new SlashPickerChoice
            {
                Value = entry.Value.Trim(),
                Label = string.IsNullOrWhiteSpace(entry.Label) ? entry.Value.Trim() : entry.Label.Trim(),
            })
            .ToList();

    public static IReadOnlyList<SlashPickerChoice> FromEnum<TEnum>()
        where TEnum : struct, Enum =>
        Enum.GetNames<TEnum>()
            .Select(name => new SlashPickerChoice
            {
                Value = name,
                Label = name,
            })
            .ToList();
}
