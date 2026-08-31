#nullable enable
using System.Text;

namespace AIGuiders.Platform.CommandPlane;

/// <summary>Factories for catalog command sources (GUIDERS-ADR-0013).</summary>
public static class CommandSource
{
    public static ICommandSource From(
        IEnumerable<CommandDescriptor> descriptors,
        string? sourceId = null) =>
        new DescriptorCommandSource(
            sourceId ?? "descriptors",
            descriptors as IReadOnlyList<CommandDescriptor> ?? descriptors.ToList());

    public static ICommandSource From(
        Func<IReadOnlyList<CommandDescriptor>> loader,
        string? sourceId = null) =>
        new DelegateCommandSource(sourceId ?? "delegate", loader);

    public static ICommandSource FromText(
        string text,
        ICommandFormatReader reader,
        string? sourceId = null) =>
        new TextCommandSource(
            sourceId ?? reader.FormatName,
            text,
            reader);

    public static ICommandSource FromStream(
        Stream stream,
        ICommandFormatReader reader,
        string? sourceId = null,
        bool leaveOpen = false) =>
        new StreamCommandSource(
            sourceId ?? reader.FormatName,
            stream,
            reader,
            leaveOpen);

    public static ICommandSource FromFile(
        string path,
        ICommandFormatReader reader,
        string? sourceId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return FromText(File.ReadAllText(path), reader, sourceId ?? $"file:{Path.GetFileName(path)}");
    }

    sealed class DescriptorCommandSource(string sourceId, IReadOnlyList<CommandDescriptor> descriptors)
        : ICommandSource
    {
        public string SourceId { get; } = sourceId;

        public IReadOnlyList<CommandDescriptor> Load() => descriptors;
    }

    sealed class DelegateCommandSource(string sourceId, Func<IReadOnlyList<CommandDescriptor>> loader)
        : ICommandSource
    {
        public string SourceId { get; } = sourceId;

        public IReadOnlyList<CommandDescriptor> Load() => loader();
    }

    sealed class TextCommandSource(string sourceId, string text, ICommandFormatReader reader)
        : ICommandSource
    {
        public string SourceId { get; } = sourceId;

        public IReadOnlyList<CommandDescriptor> Load() => reader.Read(text);
    }

    sealed class StreamCommandSource(
        string sourceId,
        Stream stream,
        ICommandFormatReader reader,
        bool leaveOpen)
        : ICommandSource
    {
        public string SourceId { get; } = sourceId;

        public IReadOnlyList<CommandDescriptor> Load()
        {
            using var textReader = new StreamReader(
                stream,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true,
                leaveOpen: true);
            var text = textReader.ReadToEnd();
            if (!leaveOpen)
            {
                stream.Dispose();
            }

            return reader.Read(text);
        }
    }
}
