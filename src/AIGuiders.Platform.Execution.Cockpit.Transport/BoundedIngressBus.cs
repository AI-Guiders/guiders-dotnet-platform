#nullable enable

using System.Threading.Channels;

namespace AIGuiders.Platform.Execution.Cockpit.Transport;

/// <summary>
/// Process-local bounded ingress bus with drop-oldest backpressure (CIDE ADR 0094 spirit).
/// </summary>
public sealed class BoundedIngressBus<TEvent> : IDisposable
{
    public const int DefaultCapacity = 64;

    readonly Channel<TEvent> _channel;
    long _published;
    long _dropped;

    public BoundedIngressBus(int capacity = DefaultCapacity)
    {
        _channel = Channel.CreateBounded<TEvent>(new BoundedChannelOptions(Math.Max(1, capacity))
        {
            SingleReader = false,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.DropOldest
        });
    }

    public ChannelReader<TEvent> Reader => _channel.Reader;

    public long Published => Interlocked.Read(ref _published);

    public long Dropped => Interlocked.Read(ref _dropped);

    public bool TryPublish(TEvent evt)
    {
        if (_channel.Writer.TryWrite(evt))
        {
            Interlocked.Increment(ref _published);
            return true;
        }

        Interlocked.Increment(ref _dropped);
        return false;
    }

    public void Dispose() => _channel.Writer.TryComplete();
}
