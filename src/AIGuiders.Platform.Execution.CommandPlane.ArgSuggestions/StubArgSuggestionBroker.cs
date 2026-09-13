#nullable enable

namespace AIGuiders.Platform.Execution.CommandPlane.ArgSuggestions;

/// <summary>Conformance / test stub map for federated suggestion providers.</summary>
public sealed class StubArgSuggestionBroker : ICommandArgSuggestionBroker
{
    readonly IReadOnlyDictionary<string, IReadOnlyList<CommandPickerChoice>> _choicesById;

    public StubArgSuggestionBroker(IReadOnlyDictionary<string, IReadOnlyList<CommandPickerChoice>> choicesById) =>
        _choicesById = choicesById;

    public IReadOnlyList<CommandPickerChoice> GetSuggestions(ArgSuggestionRequest request)
    {
        if (!_choicesById.TryGetValue(request.SuggestionId, out var choices))
        {
            return [];
        }

        if (string.IsNullOrWhiteSpace(request.Partial))
        {
            return choices;
        }

        return choices
            .Where(choice => Matches(choice, request.Partial))
            .ToList();
    }

        static bool Matches(CommandPickerChoice choice, string partial) =>
        choice.Value.Contains(partial, StringComparison.OrdinalIgnoreCase)
        || ContainsOpt(choice.Label, partial)
        || ContainsOpt(choice.Hint, partial);

    static bool ContainsOpt(Microsoft.FSharp.Core.FSharpOption<string>? value, string partial) =>
        value is not null
        && Microsoft.FSharp.Core.FSharpOption<string>.get_IsSome(value)
        && value.Value.Contains(partial, StringComparison.OrdinalIgnoreCase);
}
