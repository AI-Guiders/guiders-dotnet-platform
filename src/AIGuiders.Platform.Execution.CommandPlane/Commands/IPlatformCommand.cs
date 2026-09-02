#nullable enable

namespace AIGuiders.Platform.Execution.CommandPlane.Commands;

/// <summary>GoF Command — one identity, one <see cref="ExecuteAsync"/> path (GUIDERS-ADR-0009).</summary>
public interface IPlatformCommand<TContext> where TContext : ICommandContext
{
    string CommandId { get; }

    bool CanExecute(TContext context);

    ValueTask<CommandOutcome> ExecuteAsync(TContext context, CancellationToken cancellationToken = default);
}
