#nullable enable

namespace AIGuiders.Platform.Cockpit.DataBus;

/// <summary>Typed in-process event bus (CIDE ADR 0099 parity; no UI framework).</summary>
public interface IDataBus
{
    void Publish<TEvent>(TEvent evt);

    IDisposable Subscribe<TEvent>(Action<TEvent> handler);
}
