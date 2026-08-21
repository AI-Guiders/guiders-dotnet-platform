#nullable enable

namespace AIGuiders.Platform.Cockpit.DataBus;

/// <summary>Git segment text for IDE Health fold (ADR 0099 quarry).</summary>
public readonly record struct GitStateChanged(string Line, string CockpitShort);
