#nullable enable

using AIGuiders.Platform.Modeling.Cockpit.DataBus;

namespace AIGuiders.Platform.Execution.Cockpit.DataBus;

/// <summary>Typed in-process event bus runtime (CIDE ADR 0099). Event shapes: <see cref="AIGuiders.Platform.Modeling.Cockpit.DataBus"/>.</summary>
public interface IDataBus
{
    void Publish<TEvent>(TEvent evt);

    IDisposable Subscribe<TEvent>(Action<TEvent> handler);
}
