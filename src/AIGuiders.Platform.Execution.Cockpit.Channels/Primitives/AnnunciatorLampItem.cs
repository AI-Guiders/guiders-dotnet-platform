#nullable enable

namespace AIGuiders.Platform.Execution.Cockpit.Channels.Primitives;

/// <summary>One annunciator / Korry cell on a lamp strip (ADR 0063).</summary>
public sealed record AnnunciatorLampItem(
    string Id,
    string Title,
    string Detail,
    AnnunciatorLampLevel Level,
    string LampShortLabel);
