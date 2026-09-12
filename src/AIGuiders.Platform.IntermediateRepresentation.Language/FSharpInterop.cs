#nullable enable

using Microsoft.FSharp.Core;

namespace AIGuiders.Platform.IntermediateRepresentation.Language;

internal static class FSharpInterop
{
    internal static string? OptString(FSharpOption<string> value) =>
        FSharpOption<string>.get_IsSome(value) ? value.Value : null;

    internal static FSharpOption<string> OptString(string? value) =>
        value is null ? FSharpOption<string>.None : FSharpOption<string>.Some(value);

    internal static int? OptInt(FSharpOption<int> value) =>
        FSharpOption<int>.get_IsSome(value) ? value.Value : null;

    internal static FSharpOption<int> OptInt(int? value) =>
        value is null ? FSharpOption<int>.None : FSharpOption<int>.Some(value.Value);
}
