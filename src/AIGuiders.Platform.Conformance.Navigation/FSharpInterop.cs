#nullable enable

using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

namespace AIGuiders.Platform.Conformance.Navigation;

internal static class FSharpInterop
{
    internal static string? OptString(FSharpOption<string>? value) =>
        value is not null && FSharpOption<string>.get_IsSome(value) ? value.Value : null;

    internal static int? OptInt(FSharpOption<int>? value) =>
        value is not null && FSharpOption<int>.get_IsSome(value) ? value.Value : null;

    internal static IReadOnlyList<T> ToReadOnlyList<T>(FSharpList<T> list) =>
        ListModule.ToArray(list);

    internal static FSharpList<T> ToFSharpList<T>(IReadOnlyList<T> list) =>
        list.Count == 0 ? [] : ListModule.OfSeq(list);

    internal static FSharpList<Tuple<string, int>> ToFSharpTupleList(
        IReadOnlyDictionary<string, int>? dictionary)
    {
        if (dictionary is null or { Count: 0 })
            return [];

        return ListModule.OfSeq(dictionary.Select(pair => Tuple.Create(pair.Key, pair.Value)));
    }
}
