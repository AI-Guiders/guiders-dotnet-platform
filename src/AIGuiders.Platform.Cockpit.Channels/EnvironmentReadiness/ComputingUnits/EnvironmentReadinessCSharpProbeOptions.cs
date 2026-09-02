#nullable enable

namespace AIGuiders.Platform.Cockpit.Channels.EnvironmentReadiness.ComputingUnits;

/// <summary>C# LSP row mode for ER lamp strip (product-specific messaging).</summary>
public readonly record struct EnvironmentReadinessCSharpProbeOptions(
    bool InProcessRoslynEnabled = false,
    string? InProcessRoslynDetail = null);
