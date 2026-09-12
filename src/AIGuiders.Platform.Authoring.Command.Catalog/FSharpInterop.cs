#nullable enable

using System.Collections.Generic;
using System.Linq;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

namespace AIGuiders.Platform.Authoring.Command.Catalog;

internal static class FSharpInterop
{
    internal static string? OptString(FSharpOption<string> value) =>
        FSharpOption<string>.get_IsSome(value) ? value.Value : null;

    internal static FSharpOption<string> OptString(string? value) =>
        string.IsNullOrEmpty(value) ? FSharpOption<string>.None : FSharpOption<string>.Some(value!);

    internal static FSharpList<T> ToList<T>(IEnumerable<T> items) =>
        ListModule.OfSeq(items);

    internal static IReadOnlyList<T> FromList<T>(FSharpList<T> items) =>
        ListModule.ToArray(items);

    internal static FSharpMap<string, string> ToMap(IReadOnlyDictionary<string, string> dict)
    {
        var pairs = new Tuple<string, string>[dict.Count];
        var i = 0;
        foreach (var pair in dict)
        {
            pairs[i++] = Tuple.Create(pair.Key, pair.Value);
        }

        return MapModule.OfArray(pairs);
    }

    internal static IReadOnlyDictionary<string, string> FromMap(FSharpMap<string, string> map)
    {
        var dict = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in MapModule.ToArray(map))
        {
            dict[pair.Item1] = pair.Item2;
        }

        return dict;
    }
}
