#nullable enable

using AIGuiders.Platform.Modeling.Notations.Keyboard;
using Microsoft.FSharp.Core;

namespace AIGuiders.Platform.IntermediateRepresentation.Binding;

internal static class FSharpInterop
{
    internal static string? OptString(FSharpOption<string>? value) =>
        value is not null && FSharpOption<string>.get_IsSome(value) ? value.Value : null;

    internal static NormalizedKeySequence? OptGesture(FSharpOption<NormalizedKeySequence>? value) =>
        value is not null && FSharpOption<NormalizedKeySequence>.get_IsSome(value) ? value.Value : null;

    internal static FSharpOption<NormalizedKeySequence> OptGesture(NormalizedKeySequence? value) =>
        value is null ? FSharpOption<NormalizedKeySequence>.None : FSharpOption<NormalizedKeySequence>.Some(value);
}
