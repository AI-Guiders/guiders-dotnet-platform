#nullable enable

namespace AIGuiders.Platform.Execution.Cockpit.Transport;

/// <summary>Typed ingress wire event (CIDE ADR 0094) — MCP/cockpit request into the transport layer.</summary>
public readonly record struct IngressEvent(
    string Source,
    string? CmdLine,
    string? Verb,
    DateTimeOffset Utc);
