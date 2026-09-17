#nullable disable

using AIGuiders.Platform.IntermediateRepresentation.Command;
using AIGuiders.Platform.Modeling.CommandPlane;
using Microsoft.FSharp.Core;

namespace AIGuiders.Platform.Execution.CommandPlane.ArgSuggestions;

/// <summary>Attach pipeline step picker — maps <c>federation.attach.step.*</c> to schema prompts (plan §4.2).</summary>
public sealed class AttachStepArgSuggestionProvider : IArgSuggestionProvider
{
    public IReadOnlyList<CommandPickerChoice> GetSuggestions(ArgSuggestionRequest request)
    {
        var entry = AttachSchemaCatalog.tryFindStepBySuggestionId(request.SuggestionId);
        if (entry is null || FSharpOption<AttachSchemaStepEntry>.get_IsNone(entry))
            return [];

        var stepEntry = entry.Value;
        var choice = CommandPickerChoiceInterop.FromValue(
            stepEntry.Step.Id,
            stepEntry.Step.Prompt);

        if (string.IsNullOrWhiteSpace(request.Partial))
            return [choice];

        return Matches(choice, request.Partial) ? [choice] : [];
    }

    static bool Matches(CommandPickerChoice choice, string partial) =>
        choice.Value.Contains(partial, StringComparison.OrdinalIgnoreCase)
        || (choice.Label is not null
            && FSharpOption<string>.get_IsSome(choice.Label)
            && choice.Label.Value.Contains(partial, StringComparison.OrdinalIgnoreCase));
}
