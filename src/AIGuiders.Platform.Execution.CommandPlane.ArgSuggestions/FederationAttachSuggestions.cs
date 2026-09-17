#nullable disable

using AIGuiders.Platform.IntermediateRepresentation.Command;
using AIGuiders.Platform.Modeling.CommandPlane;
using Microsoft.FSharp.Collections;

namespace AIGuiders.Platform.Execution.CommandPlane.ArgSuggestions;

/// <summary>Attach verb picker backed by <see cref="AttachSchemaCatalog"/> (plan §4.3).</summary>
public sealed class AttachVerbArgSuggestionProvider : IArgSuggestionProvider
{
    public IReadOnlyList<CommandPickerChoice> GetSuggestions(ArgSuggestionRequest request)
    {
        var choices = AttachSchemaCatalog.schemas
            .Select(schema =>
            {
                var wire = AttachSchemaCatalog.verbWireName(schema.Verb);
                var prompt = schema.Steps?.Cast<AttachSchemaStep>().FirstOrDefault()?.Prompt ?? wire;
                return CommandPickerChoiceInterop.FromValue(wire, prompt);
            })
            .ToList();

        if (string.IsNullOrWhiteSpace(request.Partial))
            return choices;

        return choices
            .Where(choice =>
                choice.Value.Contains(request.Partial, StringComparison.OrdinalIgnoreCase)
                || LabelContains(choice, request.Partial))
            .ToList();
    }

    static bool LabelContains(CommandPickerChoice choice, string partial) =>
        choice.Label is not null
        && Microsoft.FSharp.Core.FSharpOption<string>.get_IsSome(choice.Label)
        && choice.Label.Value.Contains(partial, StringComparison.OrdinalIgnoreCase);
}

/// <summary>Default federation attach suggestion registry.</summary>
public static class FederationAttachSuggestions
{
    public static ICommandArgSuggestionBroker CreateBroker() =>
        new CommandArgSuggestionRegistry()
            .RegisterExact(AttachSchemaCatalog.VerbSuggestionId, new AttachVerbArgSuggestionProvider())
            .RegisterPrefix("federation.attach.step.", new AttachStepArgSuggestionProvider())
            .Build();
}
