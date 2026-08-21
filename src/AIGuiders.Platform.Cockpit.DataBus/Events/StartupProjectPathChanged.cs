#nullable enable

namespace AIGuiders.Platform.Cockpit.DataBus;

/// <summary>Startup project path for F5/scope in IDE Health (ADR 0099 quarry).</summary>
public readonly record struct StartupProjectPathChanged(string? ProjectPath);
