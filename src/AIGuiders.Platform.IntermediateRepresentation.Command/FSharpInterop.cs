#nullable enable

using Microsoft.FSharp.Core;

namespace AIGuiders.Platform.IntermediateRepresentation.Command;

internal static class FSharpInterop
{
    internal static string? OptString(FSharpOption<string> value) =>
        FSharpOption<string>.get_IsSome(value) ? value.Value : null;

    internal static FSharpOption<string> OptString(string? value) =>
        string.IsNullOrEmpty(value) ? FSharpOption<string>.None : FSharpOption<string>.Some(value!);
}
