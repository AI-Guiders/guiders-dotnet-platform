#nullable enable

namespace AIGuiders.Platform.Sources;

/// <summary>Factories for <see cref="ISource{T}"/> and merge combinators (GUIDERS-ADR-0029).</summary>
public static class SourceCatalog
{
    public static ISource<T> From<T>(T value, string? sourceId = null) =>
        new ValueSource<T>(sourceId ?? "value", value);

    public static ISource<T> From<T>(Func<T> loader, string? sourceId = null) =>
        new DelegateSource<T>(sourceId ?? "delegate", loader);

    public static ISource<TOut> FromText<TOut>(
        string text,
        IFormatReader<TOut> reader,
        string? sourceId = null) =>
        new TextSource<TOut>(sourceId ?? reader.FormatName, text, reader);

    public static ISource<TOut> FromFile<TOut>(
        string path,
        IFormatReader<TOut> reader,
        string? sourceId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return FromText(File.ReadAllText(path), reader, sourceId ?? $"file:{Path.GetFileName(path)}");
    }

    /// <summary>Overlay merge: first source is baseline, each next overlays via <paramref name="combiner"/>.</summary>
    public static ISource<T> Merge<T>(
        ISource<T> baseline,
        ISource<T> overlay,
        Func<T, T, T> combiner,
        string? sourceId = null) =>
        Merge([baseline, overlay], combiner, sourceId);

    /// <summary>Ordered merge: index 0 = baseline, each layer overlays the accumulator.</summary>
    public static ISource<T> Merge<T>(
        IReadOnlyList<ISource<T>> layers,
        Func<T, T, T> combiner,
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

    sealed class ValueSource<T>(string sourceId, T value) : ISource<T>
    {
        public string SourceId { get; } = sourceId;
        public T Load() => value;
    }

    sealed class DelegateSource<T>(string sourceId, Func<T> loader) : ISource<T>
    {
        public string SourceId { get; } = sourceId;
        public T Load() => loader();
    }

    sealed class TextSource<TOut>(string sourceId, string text, IFormatReader<TOut> reader) : ISource<TOut>
    {
        public string SourceId { get; } = sourceId;
        public TOut Load() => reader.Read(text);
    }

    sealed class MergedSource<T>(
        string sourceId,
        IReadOnlyList<ISource<T>> layers,
        Func<T, T, T> combiner) : ISource<T>
    {
        public string SourceId { get; } = sourceId;

        public T Load()
        {
            var acc = layers[0].Load();
            for (var i = 1; i < layers.Count; i++)
                acc = combiner(acc, layers[i].Load());
            return acc;
        }
    }
}
