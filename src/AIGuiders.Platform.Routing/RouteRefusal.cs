using AIGuiders.Platform.Abstractions;

namespace AIGuiders.Platform.Routing;

/// <summary>Shared route refusal helpers for organs.</summary>
public static class RouteRefusal
{
    public static TRoute Refuse<TRoute>(
        Func<string, string, string?, TRoute> factory,
        string raw,
        string reason,
        string? go = null)
        => factory(raw, reason, go);

    public static IntentOutcome OutcomeNotOk(RoutedIntent route, string? pulse = null)
        => new(
            route.Raw,
            route.Verb,
            Ok: false,
            Go: route.Go,
            Cmd: route.Cmd,
            Pulse: pulse ?? route.Reason,
            Reason: route.Reason ?? "route_not_ok");
}
