#nullable enable
using AIGuiders.Platform.CommandPlane;

namespace AIGuiders.Platform.CommandPlane.Sources;

/// <summary>File-backed <see cref="ICommandSource"/> factories — format resolved by extension (GUIDERS-ADR-0013).</summary>
public static class FileCommandSources
{
    public static ICommandSource FromFile(string path, string? sourceId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var format = CommandSourceFormats.Resolve(path);
        return CommandSource.FromFile(path, CommandFormatReaders.For(format), sourceId ?? $"file:{Path.GetFileName(path)}");
    }

    public static ICommandSource From(
        string content,
        CommandDocumentFormat format,
        string? sourceId = null) =>
        CommandSource.FromText(content, CommandFormatReaders.For(format), sourceId ?? format.ToString().ToLowerInvariant());
}
