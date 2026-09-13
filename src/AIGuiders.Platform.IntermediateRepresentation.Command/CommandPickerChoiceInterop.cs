#nullable enable

namespace AIGuiders.Platform.IntermediateRepresentation.Command;

/// <summary>Execution-side helpers for F# <see cref="CommandPickerChoice"/> (ADR-0003 cutover).</summary>
public static class CommandPickerChoiceInterop
{
    public static string? LabelOrNull(this CommandPickerChoice choice) =>
        FSharpInterop.OptString(choice.Label);

    public static string? HintOrNull(this CommandPickerChoice choice) =>
        FSharpInterop.OptString(choice.Hint);

    public static CommandPickerChoice FromValue(
        string value,
        string? label = null,
        string? hint = null,
        CommandPickerChoiceKind kind = CommandPickerChoiceKind.Value) =>
        new(
            value.Trim(),
            FSharpInterop.OptString(label),
            FSharpInterop.OptString(hint),
            kind);
}
