using AIGuiders.Platform.IntermediateRepresentation.Command;
#nullable enable

namespace AIGuiders.Platform.CommandPlane.ArgSuggestions;

/// <summary>Platform broker: routes suggestion ids to registered planet providers.</summary>
public interface ICommandArgSuggestionBroker
{
    IReadOnlyList<CommandPickerChoice> GetSuggestions(ArgSuggestionRequest request);
}
