#nullable enable

namespace AIGuiders.Platform.Cockpit.Channels.Primitives;

/// <summary>W/C/A annunciator lamp level (ADR 0021 / EICAS grammar).</summary>
public enum AnnunciatorLampLevel
{
    Ok,
    Advisory,
    Caution,
    Critical,
}
