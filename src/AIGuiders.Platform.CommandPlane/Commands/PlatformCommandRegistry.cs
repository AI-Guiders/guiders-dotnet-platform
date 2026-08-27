#nullable enable

namespace AIGuiders.Platform.CommandPlane.Commands;

/// <summary>Registry of <see cref="IPlatformCommand{TContext}"/> by <see cref="IPlatformCommand{TContext}.CommandId"/>.</summary>
public sealed class PlatformCommandRegistry<TContext> where TContext : ICommandContext
{
    private readonly Dictionary<string, IPlatformCommand<TContext>> _commands =
        new(StringComparer.OrdinalIgnoreCase);

    public void Register(IPlatformCommand<TContext> command)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (string.IsNullOrWhiteSpace(command.CommandId))
            throw new ArgumentException("CommandId is required.", nameof(command));

        _commands[command.CommandId] = command;
    }

    public bool Contains(string commandId) =>
        !string.IsNullOrWhiteSpace(commandId) && _commands.ContainsKey(commandId);

    public bool TryGet(string commandId, out IPlatformCommand<TContext>? command)
    {
        if (string.IsNullOrWhiteSpace(commandId))
        {
            command = null;
            return false;
        }

        return _commands.TryGetValue(commandId, out command);
    }

    public bool TryExecute(
        string commandId,
        TContext context,
        out CommandOutcome outcome,
        CancellationToken cancellationToken = default)
    {
        if (!TryGet(commandId, out var command) || command is null)
        {
            outcome = CommandOutcome.Fail($"Unknown command: {commandId}");
            return false;
        }

        if (!command.CanExecute(context))
        {
            outcome = CommandOutcome.Fail($"Command cannot execute: {commandId}");
            return false;
        }

        outcome = command.ExecuteAsync(context, cancellationToken).GetAwaiter().GetResult();
        return outcome.Success;
    }

    public IReadOnlyCollection<string> CommandIds => _commands.Keys.ToList();
}
