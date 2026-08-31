#nullable enable

namespace AIGuiders.Platform.CommandPlane;

/// <summary>
/// Product adapter for dynamic slash picker choices (<c>ArgTail = picker:&lt;id&gt;</c> without static
/// <see cref="CommandDescriptor.ArgPickerChoices"/>).
/// </summary>
public interface ICommandPickerChoiceSource
{
    IReadOnlyList<CommandPickerChoice> GetChoices(string pickerId, string partial);
}
