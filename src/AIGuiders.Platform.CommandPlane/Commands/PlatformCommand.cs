#nullable enable

namespace AIGuiders.Platform.CommandPlane.Commands;

/// <summary>Sync GoF command base — editor buffer and other local commands.</summary>
public abstract class PlatformCommand<TContext> : IPlatformCommand<TContext> where TContext : ICommandContext
{
    public abstract string CommandId { get; }

    public virtual bool CanExecute(TContext context) => context is not null;

    public ValueTask<CommandOutcome> ExecuteAsync(TContext context, CancellationToken cancellationToken = default) =>
        new(Execute(context));

    protected abstract CommandOutcome Execute(TContext context);
}
