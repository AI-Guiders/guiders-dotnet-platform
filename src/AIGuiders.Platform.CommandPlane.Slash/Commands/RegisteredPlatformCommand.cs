#nullable enable

namespace AIGuiders.Platform.CommandPlane.Commands;

/// <summary>Registered command + optional explicit catalog row.</summary>
public sealed record RegisteredPlatformCommand<TContext>(
    IPlatformCommand<TContext> Command,
    CommandDescriptor? ExplicitDescriptor = null)
    where TContext : ICommandContext
{
    public CommandDescriptor? TryResolveDescriptor()
    {
        if (ExplicitDescriptor is not null)
        {
            return ExplicitDescriptor;
        }

        return Command is ICatalogDescribed described
            ? described.ToSlashDescriptor()
            : null;
    }
}
