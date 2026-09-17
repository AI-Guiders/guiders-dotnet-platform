#nullable disable

using AIGuiders.Platform.Execution.Ide.Session;
using AIGuiders.Platform.IntermediateRepresentation.Command;
using AIGuiders.Platform.Modeling.Ide.Session;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

namespace AIGuiders.Platform.Execution.CommandPlane.ArgSuggestions;

/// <summary>Contextual member picker from session G semantic symbols (plan §4.4).</summary>
public sealed class AttachSemanticMemberArgSuggestionProvider : IArgSuggestionProvider
{
    readonly IAttachSessionAccessor _sessionAccessor;
    readonly AttachStepArgSuggestionProvider _fallback = new();

    public AttachSemanticMemberArgSuggestionProvider(IAttachSessionAccessor sessionAccessor = null) =>
        _sessionAccessor = sessionAccessor ?? FederationAttachSessionAccessor.Instance;

    public IReadOnlyList<CommandPickerChoice> GetSuggestions(ArgSuggestionRequest request)
    {
        var runtime = _sessionAccessor.TryGet(request.WorkspaceAnchor);
        if (runtime is null)
            return _fallback.GetSuggestions(request);

        var choices = SessionGraphPickerChoices.semanticSymbolPickerChoices(runtime)
            .Select(choice => CommandPickerChoiceInterop.FromValue(choice.Id, choice.Label))
            .ToList();

        if (choices.Count == 0)
            return _fallback.GetSuggestions(request);

        return FilterChoices(choices, request.Partial);
    }

    static IReadOnlyList<CommandPickerChoice> FilterChoices(
        IReadOnlyList<CommandPickerChoice> choices,
        string partial)
    {
        if (string.IsNullOrWhiteSpace(partial))
            return choices;

        return choices
            .Where(choice => AttachPickerChoiceFilter.Matches(choice, partial))
            .ToList();
    }
}

/// <summary>Manual attach kind picker — RelationSpec case names (plan §4.3).</summary>
public sealed class AttachManualKindArgSuggestionProvider : IArgSuggestionProvider
{
    readonly AttachStepArgSuggestionProvider _fallback = new();

    public IReadOnlyList<CommandPickerChoice> GetSuggestions(ArgSuggestionRequest request)
    {
        var choices = SessionGraphPickerChoices.relationSpecKindChoices
            .Select(choice => CommandPickerChoiceInterop.FromValue(choice.Id, choice.Label))
            .ToList();

        if (choices.Count == 0)
            return _fallback.GetSuggestions(request);

        if (string.IsNullOrWhiteSpace(request.Partial))
            return choices;

        return choices
            .Where(choice => AttachPickerChoiceFilter.Matches(choice, request.Partial))
            .ToList();
    }
}

/// <summary>Manual attach browse-all picker — unfiltered session targets (plan §4.4).</summary>
public sealed class AttachManualBrowseAllArgSuggestionProvider : IArgSuggestionProvider
{
    readonly IAttachSessionAccessor _sessionAccessor;
    readonly AttachStepArgSuggestionProvider _fallback = new();

    public AttachManualBrowseAllArgSuggestionProvider(IAttachSessionAccessor sessionAccessor = null) =>
        _sessionAccessor = sessionAccessor ?? FederationAttachSessionAccessor.Instance;

    public IReadOnlyList<CommandPickerChoice> GetSuggestions(ArgSuggestionRequest request)
    {
        var runtime = _sessionAccessor.TryGet(request.WorkspaceAnchor);
        if (runtime is null)
            return _fallback.GetSuggestions(request);

        var choices = SessionGraphPickerChoices.browseAllPickerChoices(runtime)
            .Select(choice => CommandPickerChoiceInterop.FromValue(choice.Id, choice.Label))
            .ToList();

        if (choices.Count == 0)
            return _fallback.GetSuggestions(request);

        if (string.IsNullOrWhiteSpace(request.Partial))
            return choices;

        return choices
            .Where(choice => AttachPickerChoiceFilter.Matches(choice, request.Partial))
            .ToList();
    }
}

static file class AttachPickerChoiceFilter
{
    public static bool Matches(CommandPickerChoice choice, string partial) =>
        choice.Value.Contains(partial, StringComparison.OrdinalIgnoreCase)
        || (choice.Label is not null
            && FSharpOption<string>.get_IsSome(choice.Label)
            && choice.Label.Value.Contains(partial, StringComparison.OrdinalIgnoreCase));
}
