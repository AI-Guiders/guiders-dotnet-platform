#nullable enable

using Microsoft.FSharp.Core;

namespace AIGuiders.Platform.Combinations;

/// <summary>Combines baseline with one overlay layer. Policy lives in the delegate implementation.</summary>
public delegate T Combinator<T>(T baseline, T overlay);

internal static class CombinatorBridge
{
    internal static FSharpFunc<T, FSharpFunc<T, T>> ToFSharp<T>(Combinator<T> combiner) =>
        FSharpFunc<T, FSharpFunc<T, T>>.FromConverter(
            baseline => FSharpFunc<T, T>.FromConverter(overlay => combiner(baseline, overlay)));
}
