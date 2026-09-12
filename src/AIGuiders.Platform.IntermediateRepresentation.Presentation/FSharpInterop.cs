#nullable enable

using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

namespace AIGuiders.Platform.IntermediateRepresentation.Presentation;

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

    internal static double? OptDouble(FSharpOption<double> value) =>
        FSharpOption<double>.get_IsSome(value) ? value.Value : null;

    internal static FSharpOption<double> OptDouble(double? value) =>
        value is null ? FSharpOption<double>.None : FSharpOption<double>.Some(value.Value);

    internal static IReadOnlyList<T> FromList<T>(FSharpList<T> list) =>
        ListModule.ToArray(list);

    internal static FSharpList<T> ToFSharpList<T>(IEnumerable<T> items) =>
        ListModule.OfSeq(items);
}
