#nullable enable

namespace AIGuiders.Platform.Execution.Sources;

public static class FileSources
{
    public static ISource<TOut> FromFile<TOut>(
        string path,
        IFormatReader<TOut> reader,
        string? sourceId = null) =>
        SourceCatalog.FromFile(path, reader, sourceId);
}
