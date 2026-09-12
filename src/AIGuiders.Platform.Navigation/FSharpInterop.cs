#nullable enable

using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

namespace AIGuiders.Platform.Navigation;

internal static class FSharpInterop
{
    internal static string? OptString(FSharpOption<string> value) =>
        FSharpOption<string>.get_IsSome(value) ? value.Value : null;

    internal static FSharpOption<string> OptString(string? value) =>
        string.IsNullOrEmpty(value) ? FSharpOption<string>.None : FSharpOption<string>.Some(value!);

    internal static int? OptInt(FSharpOption<int> value) =>
        FSharpOption<int>.get_IsSome(value) ? value.Value : null;

    internal static FSharpOption<int> OptInt(int? value) =>
        value.HasValue ? FSharpOption<int>.Some(value.Value) : FSharpOption<int>.None;

    internal static IReadOnlyList<T> ToReadOnlyList<T>(FSharpList<T> list) =>
        ListModule.ToArray(list);

    internal static FSharpList<T> ToFSharpList<T>(IReadOnlyList<T>? list) =>
        list is null or { Count: 0 }
            ? []
            : ListModule.OfSeq(list);

    internal static IReadOnlyDictionary<string, int>? ToReadOnlyDict(FSharpOption<FSharpMap<string, int>> value) =>
        FSharpOption<FSharpMap<string, int>>.get_IsSome(value)
            ? value.Value.ToDictionary(pair => pair.Key, pair => pair.Value)
            : null;

    internal static FSharpOption<FSharpMap<string, int>> ToFSharpMap(IReadOnlyDictionary<string, int>? dict) =>
        dict is null or { Count: 0 }
            ? FSharpOption<FSharpMap<string, int>>.None
            : FSharpOption<FSharpMap<string, int>>.Some(
                MapModule.OfSeq(dict.Select(kvp => Tuple.Create(kvp.Key, kvp.Value))));

    internal static IReadOnlyList<string>? OptReadOnlyStringList(FSharpOption<FSharpList<string>> value) =>
        FSharpOption<FSharpList<string>>.get_IsSome(value)
            ? ToReadOnlyList(value.Value)
            : null;

    internal static FSharpOption<FSharpList<string>> OptFSharpStringList(IReadOnlyList<string>? value) =>
        value is null or { Count: 0 }
            ? FSharpOption<FSharpList<string>>.None
            : FSharpOption<FSharpList<string>>.Some(ToFSharpList(value));
}
