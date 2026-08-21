using AIGuiders.Platform.Cockpit.Cds;
using AIGuiders.Platform.Cockpit.Channels.EnvironmentReadiness;
using AIGuiders.Platform.Cockpit.Channels.IdeHealth;
using AIGuiders.Platform.Cockpit.Channels.Primitives;
using AIGuiders.Platform.Cockpit.Composition;
using AIGuiders.Platform.Cockpit.DataBus;
using Xunit;

namespace AIGuiders.Platform.Tests;

public class CockpitDtoTests
{
    [Fact]
    public void DeskSurfaceBuiltEvent_roundtrip()
    {
        var e = new DeskSurfaceBuiltEvent("seats", 2, "nav", DateTimeOffset.UtcNow);
        Assert.Equal("seats", e.Mode);
        Assert.Equal(2, e.SeatCount);
    }

    [Fact]
    public void IdeHealthInputSnapshot_FromFlat()
    {
        var seg = new IdeHealthSegmentInput("ok", "OK");
        var snap = IdeHealthInputSnapshot.FromFlat(seg, seg, seg, seg);
        Assert.Equal("ok", snap.Solution.Build.LineText);
        Assert.Equal("ok", snap.Workspace.Git.LineText);
    }

    [Fact]
    public void AttentionRoutingDecision_payload()
    {
        var d = new AttentionRoutingDecision("nav", null, true);
        Assert.True(d.DeskDetailNavForced);
    }

    [Fact]
    public void SeatsSurfaceScene_defaults()
    {
        var scene = new SeatsSurfaceScene(
            "cockpit/v1.20", "nav", new { }, Array.Empty<object>(), new { },
            null, null, null, new { }, null, null, null, [], [], null, null, null);
        Assert.Equal("nav", scene.Mfd);
    }

    [Fact]
    public void EnvironmentReadinessSnapshot_rows()
    {
        var row = new AnnunciatorLampItem("id", "t", "d", AnnunciatorLampLevel.Ok, "OK");
        var snap = new EnvironmentReadinessSnapshot([row]);
        Assert.Single(snap.Rows);
    }
}
