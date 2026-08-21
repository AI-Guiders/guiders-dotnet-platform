#nullable enable

namespace AIGuiders.Platform.Cockpit.Channels.IdeHealth.ComputingUnits;

public readonly record struct IdeHealthScopeDecision(IdeHealthScope Scope, string? ProjectPath);
