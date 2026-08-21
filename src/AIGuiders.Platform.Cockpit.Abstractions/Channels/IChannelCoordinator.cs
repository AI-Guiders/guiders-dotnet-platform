#nullable enable

namespace AIGuiders.Platform.Cockpit.Channels;

/// <summary>Aggregates multiple channel outputs into one payload (CIDE channel coordinator seam).</summary>
public interface IChannelCoordinator<TContext, TPayload>
{
    TPayload Build(in TContext context);
}
