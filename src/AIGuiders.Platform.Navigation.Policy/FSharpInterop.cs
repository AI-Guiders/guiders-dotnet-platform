#nullable enable

using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

namespace AIGuiders.Platform.Navigation.Policy;

internal static class FSharpInterop
{
    internal static string? OptString(FSharpOption<string> value) =>
        FSharpOption<string>.get_IsSome(value) ? value.Value : null;

    internal static FSharpOption<string> OptString(string? value) =>
        string.IsNullOrEmpty(value) ? FSharpOption<string>.None : FSharpOption<string>.Some(value!);

    internal static IReadOnlyList<T> ToReadOnlyList<T>(FSharpList<T> list) =>
        ListModule.ToArray(list);

    internal static FSharpList<T> ToFSharpList<T>(IReadOnlyList<T>? list) =>
        list is null or { Count: 0 }
            ? []
            : ListModule.OfSeq(list);

    internal static IReadOnlyList<string>? OptReadOnlyStringList(FSharpOption<FSharpList<string>> value) =>
        FSharpOption<FSharpList<string>>.get_IsSome(value)
            ? ToReadOnlyList(value.Value)
            : null;

    internal static FSharpOption<FSharpList<string>> OptFSharpStringList(IReadOnlyList<string>? value) =>
        value is null or { Count: 0 }
            ? FSharpOption<FSharpList<string>>.None
            : FSharpOption<FSharpList<string>>.Some(ToFSharpList(value));

    internal static FSharpOption<int> OptInt(int? value) =>
        value.HasValue ? FSharpOption<int>.Some(value.Value) : FSharpOption<int>.None;
}
