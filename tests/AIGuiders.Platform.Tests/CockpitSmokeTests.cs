using AIGuiders.Platform.Execution.Cockpit.DataBus;
using AIGuiders.Platform.Execution.Cockpit.Transport;
using Xunit;

namespace AIGuiders.Platform.Tests;

public class CockpitDataBusTests
{
    [Fact]
    public void InMemoryDataBus_publish_subscribe_roundtrip()
    {
        using var bus = new InMemoryDataBus();
        var got = 0;
        using var sub = bus.Subscribe<TestEvent>(e => got = e.Value);
        bus.Publish(new TestEvent(7));
        Assert.Equal(7, got);
    }

    readonly record struct TestEvent(int Value);
}

public class CockpitIngressBusTests
{
    [Fact]
    public void BoundedIngressBus_try_publish_increments_published()
    {
        using var bus = new BoundedIngressBus<IngressEvent>();
        var ok = bus.TryPublish(new IngressEvent("test", null, "go", DateTimeOffset.UtcNow));
        Assert.True(ok);
        Assert.Equal(1, bus.Published);
        Assert.Equal(0, bus.Dropped);
    }
}
