#nullable enable

namespace AIGuiders.Platform.CommandPlane.Commands;

/// <summary>Registry of <see cref="IPlatformCommand{TContext}"/> by <see cref="IPlatformCommand{TContext}.CommandId"/>.</summary>
public sealed class PlatformCommandRegistry<TContext> where TContext : ICommandContext
{
    private readonly Dictionary<string, RegisteredPlatformCommand<TContext>> _commands =
        new(StringComparer.OrdinalIgnoreCase);

    public void Register(IPlatformCommand<TContext> command) =>
        Store(command, explicitDescriptor: null);

    public void Register(IPlatformCommand<TContext> command, CommandDescriptor explicitDescriptor)
    {
        ArgumentNullException.ThrowIfNull(explicitDescriptor);
        Store(command, explicitDescriptor);
    }

    void Store(IPlatformCommand<TContext> command, CommandDescriptor? explicitDescriptor)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (string.IsNullOrWhiteSpace(command.CommandId))
        {
            throw new ArgumentException("CommandId is required.", nameof(command));
        }

        _commands[command.CommandId] = new RegisteredPlatformCommand<TContext>(command, explicitDescriptor);
    }

    public bool Contains(string commandId) =>
        !string.IsNullOrWhiteSpace(commandId) && _commands.ContainsKey(commandId);

    public bool TryGet(string commandId, out IPlatformCommand<TContext>? command)
    {
        if (string.IsNullOrWhiteSpace(commandId)
            || !_commands.TryGetValue(commandId, out var registered))
        {
            command = null;
            return false;
        }

        command = registered.Command;
        return true;
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

    public void Accept(ICatalogVisitor visitor, Func<CommandDescriptor, bool>? predicate = null)
    {
        ArgumentNullException.ThrowIfNull(visitor);

        foreach (var registered in _commands.Values.OrderBy(x => x.Command.CommandId, StringComparer.OrdinalIgnoreCase))
        {
            var descriptor = registered.TryResolveDescriptor();
            if (descriptor is null)
            {
                continue;
            }

            if (predicate is not null && !predicate(descriptor))
            {
                continue;
            }

            if (!visitor.Visit(descriptor))
            {
                break;
            }
        }
    }

    public IReadOnlyCollection<string> CommandIds => _commands.Keys.ToList();
}
