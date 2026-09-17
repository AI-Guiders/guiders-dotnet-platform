#nullable disable

using AIGuiders.Platform.Execution.Ide.Session;
using AIGuiders.Platform.IntermediateRepresentation.Command;
using AIGuiders.Platform.Modeling.CommandPlane;
using AIGuiders.Platform.Modeling.Ide.Session;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

namespace AIGuiders.Platform.Execution.CommandPlane.ArgSuggestions;

/// <summary>
/// Contextual diagnostic picker for <c>federation.attach.step.pick_diagnostic</c> (plan §4.4).
/// Falls back to schema step prompt when session G has no diagnostics.
/// </summary>
public sealed class AttachDiagnosticArgSuggestionProvider : IArgSuggestionProvider
{
    readonly IAttachSessionAccessor _sessionAccessor;
    readonly AttachStepArgSuggestionProvider _fallback = new();

    public AttachDiagnosticArgSuggestionProvider(IAttachSessionAccessor sessionAccessor = null) =>
        _sessionAccessor = sessionAccessor ?? FederationAttachSessionAccessor.Instance;

    public IReadOnlyList<CommandPickerChoice> GetSuggestions(ArgSuggestionRequest request)
    {
        var runtime = _sessionAccessor.TryGet(request.WorkspaceAnchor);
        if (runtime is null || MapModule.IsEmpty(runtime.Diagnostics))
            return _fallback.GetSuggestions(request);

        var choices = DiagnosticIndexOps.pickerChoices(runtime.Diagnostics)
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
