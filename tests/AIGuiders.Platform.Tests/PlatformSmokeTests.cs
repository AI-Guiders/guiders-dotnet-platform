using AIGuiders.Platform.Abstractions;
using AIGuiders.Platform.Routing;
using Xunit;

namespace AIGuiders.Platform.Tests;

public class PulseFormatTests
{
    [Fact]
    public void Truncate_appends_ellipsis_when_over_max()
    {
        var s = new string('x', 300);
        var t = PulseFormat.Truncate(s, 240);
        Assert.NotNull(t);
        Assert.Equal(241, t!.Length);
        Assert.EndsWith("…", t);
    }

    [Fact]
    public void JoinBits_skips_empty()
    {
        var pulse = PulseFormat.JoinBits(["undo", null, "ok", ""]);
        Assert.Equal("undo ok", pulse);
    }
}

public class RouteRefusalTests
{
    [Fact]
    public void OutcomeNotOk_maps_route_fields()
    {
        var route = new RoutedIntent("Buffer", "read path=foo", Ok: false, Reason: "nope", Go: "buffer");
        var outcome = RouteRefusal.OutcomeNotOk(route);
        Assert.False(outcome.Ok);
        Assert.Equal("nope", outcome.Reason);
        Assert.Equal("buffer", outcome.Go);
    }
}

public class IntentOrganContractTests
{
    private sealed class EchoOrgan : IIntentOrgan<RoutedIntent, IntentOutcome>
    {
        public RoutedIntent Route(string raw) =>
            new("Echo", raw, Ok: true, Op: "echo");

        public IntentOutcome Execute(RoutedIntent route, DispatchCallOverride? callOverride = null) =>
            new(route.Raw, route.Verb, Ok: true, Action: route.Op, Pulse: "echo ok");
    }

    [Fact]
    public void Organ_route_execute_roundtrip()
    {
        var organ = new EchoOrgan();
        var route = organ.Route("hello");
        var applied = organ.Execute(route);
        Assert.True(applied.Ok);
        Assert.Equal("echo ok", applied.Pulse);
    }
}
