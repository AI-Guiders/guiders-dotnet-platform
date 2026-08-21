#nullable enable

namespace AIGuiders.Platform.Cockpit.DataBus;

/// <summary>
/// LSP stdio projection for DataBus and Environment Readiness (stratum C, ADR 0099).
/// Publish together with host attach state — do not duplicate host.IsActive elsewhere.
/// </summary>
public readonly record struct IdeHostStateChanged(
    bool CSharpLspProcessActive,
    bool MarkdownLspProcessActive,
    bool CSharpLspHostPresent,
    bool MarkdownLspHostPresent);
