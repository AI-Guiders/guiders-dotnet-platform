#nullable enable

namespace AIGuiders.Platform.CommandPlane.ArgSuggestions;

public sealed class CommandArgSuggestionBroker : ICommandArgSuggestionBroker
{
    readonly IReadOnlyList<(Func<string, bool> Match, IArgSuggestionProvider Provider)> _providers;

    public CommandArgSuggestionBroker(
        IReadOnlyList<(Func<string, bool> Match, IArgSuggestionProvider Provider)> providers) =>
        _providers = providers;

    public IReadOnlyList<CommandPickerChoice> GetSuggestions(ArgSuggestionRequest request)
    {
        foreach (var (match, provider) in _providers)
        {
            if (!match(request.SuggestionId))
            {
                continue;
            }

            return provider.GetSuggestions(request);
        }

        return [];
    }
}
