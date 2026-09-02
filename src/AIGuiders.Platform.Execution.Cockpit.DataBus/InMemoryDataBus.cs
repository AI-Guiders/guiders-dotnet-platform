#nullable enable

using System.Threading.Channels;
using AIGuiders.Platform.Modeling.Cockpit.DataBus;

namespace AIGuiders.Platform.Execution.Cockpit.DataBus;

/// <summary>In-process <see cref="IDataBus"/> with optional async dispatch (ADR 0099 W5).</summary>
public sealed class InMemoryDataBus : IDataBus, IDisposable
{
    readonly Lock _sync = new();
    readonly Dictionary<Type, List<Delegate>> _handlers = [];
    readonly Dictionary<Type, object> _routes = [];
    readonly bool _asynchronousDispatch;
    readonly CancellationTokenSource? _dispatchCts;
    readonly DispatchPolicy _eventPolicy;

    public InMemoryDataBus(bool asynchronousDispatch = false, DispatchPolicy? eventPolicy = null)
    {
        _asynchronousDispatch = asynchronousDispatch;
        _dispatchCts = asynchronousDispatch ? new CancellationTokenSource() : null;
        _eventPolicy = eventPolicy ?? DispatchPolicyModule.defaultPolicy;
    }

    public void Publish<TEvent>(TEvent evt)
    {
        if (!_asynchronousDispatch)
        {
            DispatchToSubscribers(evt);
            return;
        }

        GetOrCreateRoute<TEvent>().Publish(evt);
    }

    public IDisposable Subscribe<TEvent>(Action<TEvent> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        lock (_sync)
        {
            if (!_handlers.TryGetValue(typeof(TEvent), out var list))
            {
                list = [];
                _handlers[typeof(TEvent)] = list;
            }

            list.Add(handler);
        }

        return new Subscription(() => Unsubscribe(handler));
    }

    public void Dispose()
    {
        _dispatchCts?.Cancel();
        _dispatchCts?.Dispose();
    }

    void DispatchToSubscribers<TEvent>(TEvent evt)
    {
        Delegate[] snapshot;
        lock (_sync)
        {
            if (!_handlers.TryGetValue(typeof(TEvent), out var list) || list.Count == 0)
                return;
            snapshot = [.. list];
        }

        foreach (var del in snapshot)
        {
            if (del is not Action<TEvent> handler)
                continue;
            try
            {
                handler(evt);
            }
            catch
            {
                // Isolate subscribers.
            }
        }
    }

    EventRoute<TEvent> GetOrCreateRoute<TEvent>()
    {
        lock (_sync)
        {
            if (_routes.TryGetValue(typeof(TEvent), out var existing) && existing is EventRoute<TEvent> route)
                return route;

            var created = EventRoute<TEvent>.Create(
                this,
                DispatchPolicyModule.isBurstForTypeName(typeof(TEvent).Name, _eventPolicy),
                _dispatchCts?.Token ?? CancellationToken.None);
            _routes[typeof(TEvent)] = created;
            return created;
        }
    }

    sealed class EventRoute<TEvent>
    {
        readonly InMemoryDataBus _owner;
        readonly Channel<TEvent> _channel;
        readonly CancellationToken _cancellationToken;

        EventRoute(InMemoryDataBus owner, Channel<TEvent> channel, CancellationToken cancellationToken)
        {
            _owner = owner;
            _channel = channel;
            _cancellationToken = cancellationToken;
            _ = Task.Run(DispatchLoopAsync, CancellationToken.None);
        }

        public static EventRoute<TEvent> Create(InMemoryDataBus owner, bool latestWinsBurst, CancellationToken cancellationToken)
        {
            if (latestWinsBurst)
            {
                var bounded = Channel.CreateBounded<TEvent>(new BoundedChannelOptions(1)
                {
                    SingleReader = true,
                    SingleWriter = false,
                    FullMode = BoundedChannelFullMode.DropOldest,
                });
                return new EventRoute<TEvent>(owner, bounded, cancellationToken);
            }

            var unbounded = Channel.CreateUnbounded<TEvent>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false,
            });
            return new EventRoute<TEvent>(owner, unbounded, cancellationToken);
        }

        public void Publish(TEvent evt) => _channel.Writer.TryWrite(evt);

        async Task DispatchLoopAsync()
        {
            try
            {
                await foreach (var evt in _channel.Reader.ReadAllAsync(_cancellationToken))
                    _owner.DispatchToSubscribers(evt);
            }
            catch (OperationCanceledException)
            {
                // Expected on dispose.
            }
        }
    }

    void Unsubscribe<TEvent>(Action<TEvent> handler)
    {
        lock (_sync)
        {
            if (!_handlers.TryGetValue(typeof(TEvent), out var list))
                return;
            list.Remove(handler);
            if (list.Count == 0)
                _handlers.Remove(typeof(TEvent));
        }
    }

    sealed class Subscription(Action dispose) : IDisposable
    {
        Action? _dispose = dispose;

        public void Dispose() => Interlocked.Exchange(ref _dispose, null)?.Invoke();
    }
}
