#nullable enable

namespace AIGuiders.Platform.CommandPlane;

/// <summary>
/// Product adapter for dynamic slash picker choices (<c>ArgTail = picker:&lt;id&gt;</c> without static
/// <see cref="SlashCommandDescriptor.ArgPickerChoices"/>).
/// </summary>
public interface ISlashPickerChoiceSource
{
    IReadOnlyList<SlashPickerChoice> GetChoices(string pickerId, string partial);
}
