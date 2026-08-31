#nullable enable

using AIGuiders.Platform.IntermediateRepresentation.Command;

namespace AIGuiders.Platform.CommandPlane;

/// <summary>Multi-path catalog rows for one <c>commandId</c> (GUIDERS-ADR-0045).</summary>
public static class CommandDescriptorRows
{
    public static IReadOnlyList<CommandDescriptor> ForCommand(
        string commandId,
        IEnumerable<(string Path, string Help)> rows,
        Action<CommandDescriptorBuilder>? configureDefaults = null)
    {
        var list = new List<CommandDescriptor>();
        foreach (var (path, help) in rows)
        {
            var builder = CommandDescriptors.Describe(commandId).Path(path).Help(help);
            configureDefaults?.Invoke(builder);
            list.Add(builder.Build());
        }

        return list;
    }

    public static IReadOnlyList<CommandDescriptor> Map<T>(
        string commandId,
        IEnumerable<T> items,
        Func<T, string> path,
        Func<T, string> help,
        Action<CommandDescriptorBuilder, T>? configure = null)
    {
        var list = new List<CommandDescriptor>();
        foreach (var item in items)
        {
            var builder = CommandDescriptors.Describe(commandId)
                .Path(path(item))
                .Help(help(item));
            configure?.Invoke(builder, item);
            list.Add(builder.Build());
        }

        return list;
    }
}
