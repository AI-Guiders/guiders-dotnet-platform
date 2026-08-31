#nullable enable

using AIGuiders.Platform.CommandPlane.Commands;

namespace AIGuiders.Platform.CommandPlane;

/// <summary>Registry helpers for code-first catalog rows (GUIDERS-ADR-0045).</summary>
public static class PlatformCommandRegistryExtensions
{
    public static void RegisterCatalog<TContext>(
        this PlatformCommandRegistry<TContext> registry,
        IPlatformCommand<TContext> command,
        Action<CommandDescriptorBuilder> configure)
        where TContext : ICommandContext
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(configure);

        var builder = CommandDescriptors.Describe(command.CommandId);
        configure(builder);
        registry.Register(command, builder.Build());
    }
}
