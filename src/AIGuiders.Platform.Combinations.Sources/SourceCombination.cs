#nullable enable

using AIGuiders.Platform.Sources;

namespace AIGuiders.Platform.Combinations.Sources;

/// <summary>Merge <see cref="ISource{T}"/> layers; combinator runs after each <see cref="ISource{T}.Load"/>.</summary>
public static class SourceCombination
{
    public static ISource<T> Merge<T>(
        ISource<T> baseline,
        ISource<T> overlay,
        Combinator<T> combiner,
        string? sourceId = null) =>
        Merge([baseline, overlay], combiner, sourceId);

    public static ISource<T> Merge<T>(
        IReadOnlyList<ISource<T>> layers,
        Combinator<T> combiner,
        string? sourceId = null)
    {
        ArgumentNullException.ThrowIfNull(layers);
        if (layers.Count == 0)
            throw new ArgumentException("At least one source layer is required.", nameof(layers));
        ArgumentNullException.ThrowIfNull(combiner);
        return new MergedSource<T>(
            sourceId ?? string.Join('+', layers.Select(x => x.SourceId)),
            layers,
            combiner);
    }

    sealed class MergedSource<T>(
        string sourceId,
        IReadOnlyList<ISource<T>> layers,
        Combinator<T> combiner) : ISource<T>
    {
        public string SourceId { get; } = sourceId;

        public T Load() =>
            OrderedCombination.Fold(
                layers.Select(static x => x.Load()).ToArray(),
                combiner);
    }
}
