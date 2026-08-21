namespace Guiders.Platform.Abstractions;

/// <summary>
/// Host-execute result — product-neutral shape aligned with CDP <c>CitizenRouteHost.Applied</c>.
/// </summary>
public sealed record IntentOutcome(
    string Raw,
    string Verb,
    bool Ok,
    string? Action = null,
    string? Seat = null,
    string? Go = null,
    string? Path = null,
    string? DocId = null,
    string? Cmd = null,
    string? Pulse = null,
    string? Reason = null,
    string? Ship = null);
