#nullable enable

namespace AIGuiders.Platform.Execution.CommandPlane.ArgSuggestions;

/// <summary>Planet-owned dynamic arg suggestions for <c>suggest:&lt;id&gt;</c> / <c>picker:&lt;id&gt;</c>.</summary>
public interface IArgSuggestionProvider
{
    IReadOnlyList<CommandPickerChoice> GetSuggestions(ArgSuggestionRequest request);
}
