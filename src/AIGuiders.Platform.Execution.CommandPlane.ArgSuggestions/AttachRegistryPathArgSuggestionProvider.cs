#nullable disable

using AIGuiders.Platform.Execution.Ide.Session;
using AIGuiders.Platform.IntermediateRepresentation.Command;
using AIGuiders.Platform.Modeling.Ide.Session;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

namespace AIGuiders.Platform.Execution.CommandPlane.ArgSuggestions;

/// <summary>
/// Contextual document path picker for attach file/doc/nav steps (plan §4.4).
/// </summary>
public sealed class AttachRegistryPathArgSuggestionProvider : IArgSuggestionProvider
{
    readonly IAttachSessionAccessor _sessionAccessor;
    readonly AttachStepArgSuggestionProvider _fallback = new();

    public AttachRegistryPathArgSuggestionProvider(IAttachSessionAccessor sessionAccessor = null) =>
        _sessionAccessor = sessionAccessor ?? FederationAttachSessionAccessor.Instance;

    public IReadOnlyList<CommandPickerChoice> GetSuggestions(ArgSuggestionRequest request)
    {
        var runtime = _sessionAccessor.TryGet(request.WorkspaceAnchor);
        if (runtime is null || MapModule.IsEmpty(runtime.Registry))
            return _fallback.GetSuggestions(request);

        var choices = DocumentRegistryOps.documentPathPickerChoices(runtime.Registry)
            .Select(choice => CommandPickerChoiceInterop.FromValue(choice.Id, choice.Label))
            .ToList();

        if (string.IsNullOrWhiteSpace(request.Partial))
            return choices;

        return choices
            .Where(choice => Matches(choice, request.Partial))
            .ToList();
    }

    static bool Matches(CommandPickerChoice choice, string partial) =>
        choice.Value.Contains(partial, StringComparison.OrdinalIgnoreCase)
        || (choice.Label is not null
            && FSharpOption<string>.get_IsSome(choice.Label)
            && choice.Label.Value.Contains(partial, StringComparison.OrdinalIgnoreCase));
}
