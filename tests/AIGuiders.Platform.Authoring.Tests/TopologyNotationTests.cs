using AIGuiders.Platform.IntermediateRepresentation.Presentation;
using AIGuiders.Platform.Notations.Presentation.Topology;
using Xunit;

namespace AIGuiders.Platform.Authoring.Tests;

public sealed class TopologyNotationTests
{
    [Theory]
    [InlineData("(MFD)(F)", TopologyArrangement.MultiHost, 2, "mfd", "forward")]
    [InlineData("(P)(F)(M)", TopologyArrangement.MultiHost, 3, "pfd", "forward")]
    [InlineData("single", TopologyArrangement.SingleSurfaceCompositional, 0, null, null)]
    public void Parse_assigns_logical_hosts_in_wire_order(
        string wire,
        TopologyArrangement arrangement,
        int hostCount,
        string? firstRole,
        string? secondRole)
    {
        var result = TopologyNotation.Parse(wire);
        Assert.True(result.IsSuccess, result.Error);
        Assert.Equal(arrangement, result.Topology!.Arrangement);
        Assert.Equal(hostCount, result.Topology.HostCount);
        Assert.Equal(wire, result.Topology.SourceWire);

        if (firstRole is not null)
        {
            Assert.Equal(firstRole, result.Topology.Hosts[0].HostId);
        }

        if (secondRole is not null && hostCount > 1)
        {
            Assert.Equal(secondRole, result.Topology.Hosts[1].HostId);
        }
    }

    [Fact]
    public void Parse_one_of_single_top_level()
    {
        var result = TopologyNotation.Parse("(F/P/M)");
        Assert.True(result.IsSuccess, result.Error);
        Assert.Equal(TopologyArrangement.SingleHostOneOf, result.Topology!.Arrangement);
        var host = Assert.Single(result.Topology.Hosts);
        Assert.Equal(AttentionDisplayRole.PmOneOf, host.Role);
        Assert.Equal(ZoneComposeKind.OneOf, host.Compose);
        Assert.Equal(["f", "p", "m"], host.ChannelStack);
    }

    [Fact]
    public void Host_index_is_wire_order_not_physical_monitor()
    {
        var result = TopologyNotation.Parse("(MFD)(F)");
        Assert.Equal(0, result.Topology!.Hosts[0].HostIndex);
        Assert.Equal(1, result.Topology.Hosts[1].HostIndex);
    }
}
