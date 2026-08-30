#nullable enable

namespace AIGuiders.Platform.Sources;

/// <summary>Factories for <see cref="ISource{T}"/> (GUIDERS-ADR-0029). Layer merge: <see cref="Combinations.Sources.SourceCombination"/>.</summary>
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
}
