#nullable enable

namespace AIGuiders.Platform.CommandPlane.ArgSuggestions;

public sealed class CommandArgSuggestionRegistry
{
    readonly List<(Func<string, bool> Match, IArgSuggestionProvider Provider)> _providers = [];

    public CommandArgSuggestionRegistry Register(
        Func<string, bool> match,
        IArgSuggestionProvider provider)
    {
        ArgumentNullException.ThrowIfNull(match);
        ArgumentNullException.ThrowIfNull(provider);
        _providers.Add((match, provider));
        return this;
    }

    public CommandArgSuggestionRegistry RegisterExact(string suggestionId, IArgSuggestionProvider provider) =>
        Register(
            id => string.Equals(id, suggestionId, StringComparison.OrdinalIgnoreCase),
            provider);

    public CommandArgSuggestionRegistry RegisterPrefix(string prefix, IArgSuggestionProvider provider) =>
        Register(
            id => id.StartsWith(prefix, StringComparison.OrdinalIgnoreCase),
            provider);

    public ICommandArgSuggestionBroker Build() => new CommandArgSuggestionBroker(_providers);
}
