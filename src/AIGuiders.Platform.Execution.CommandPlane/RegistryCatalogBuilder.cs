using AIGuiders.Platform.IntermediateRepresentation.Command;
#nullable enable
using AIGuiders.Platform.Execution.CommandPlane.Commands;

namespace AIGuiders.Platform.Execution.CommandPlane;

/// <summary>Build slash catalogs by visiting command registries (GUIDERS-ADR-0010 W2c).</summary>
public static class RegistryCatalogBuilder
{
    public static IReadOnlyList<CommandDescriptor> CollectDescriptors<TContext>(
        PlatformCommandRegistry<TContext> registry,
        Func<CommandDescriptor, bool>? predicate = null)
        where TContext : ICommandContext
    {
        var collector = new CatalogDescriptorCollector();
        registry.Accept(collector, predicate);
        return collector.Descriptors;
    }

    public static CommandCatalogIndex BuildIndex<TContext>(
        PlatformCommandRegistry<TContext> registry,
        Func<CommandDescriptor, bool>? predicate = null)
        where TContext : ICommandContext =>
        CommandCatalogIndex.FromDescriptors(CollectDescriptors(registry, predicate));

    public static ICommandSource ToCommandSource<TContext>(
        PlatformCommandRegistry<TContext> registry,
        string? sourceId = null,
        Func<CommandDescriptor, bool>? predicate = null)
        where TContext : ICommandContext =>
        CommandSource.From(
            () => CollectDescriptors(registry, predicate),
            sourceId ?? $"registry:{typeof(TContext).Name}");
}
