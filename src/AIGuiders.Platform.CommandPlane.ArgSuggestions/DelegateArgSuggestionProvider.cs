#nullable enable

namespace AIGuiders.Platform.CommandPlane.ArgSuggestions;

public sealed class DelegateArgSuggestionProvider(
    Func<ArgSuggestionRequest, IReadOnlyList<CommandPickerChoice>> getSuggestions) : IArgSuggestionProvider
{
    public IReadOnlyList<CommandPickerChoice> GetSuggestions(ArgSuggestionRequest request) =>
        getSuggestions(request);
}
