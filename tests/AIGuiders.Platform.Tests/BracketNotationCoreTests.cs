#nullable enable
using AIGuiders.Platform.Notations.Bracket;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class BracketNotationCoreTests
{
    [Fact]
    public void CdpSquareKeyValue_profile_matches_BracketLocate_defaults()
    {
        var profile = BracketProfiles.CdpSquareKeyValue;
        Assert.Equal("[", profile.StartTerminal);
        Assert.Equal("]", profile.EndTerminal);
        Assert.Equal(';', profile.AxisSeparator);
        Assert.Equal(':', profile.PairDelimiter);
        Assert.True(profile.RespectBracketDepthOnAxisSplit);
        Assert.Contains("Anchor", profile.NestedAxisKeys!);
    }

    [Fact]
    public void CdpCode_value_plan_maps_scope_to_argument_colon()
    {
        Assert.Equal(
            BracketAxisValueClasses.ArgumentColon,
            BracketAxisValuePlans.CdpCode.ByAxisKey["S"]);
        Assert.Equal(
            BracketAxisValueClasses.CommandPath,
            BracketAxisValuePlans.CdpCode.ByAxisKey["F"]);
    }

    [Fact]
    public void ForgeFrg_value_plan_uses_command_path()
    {
        Assert.Equal(
            BracketAxisValueClasses.CommandPath,
            BracketAxisValuePlans.ForgeFrgCompound.ByAxisKey["FRG"]);
    }

    [Fact]
    public void BracketAxis_carries_value_wire_class_for_compose()
    {
        var axis = new BracketAxis("S", "for:2", BracketAxisValueClasses.ArgumentColon);
        Assert.Equal("for:2", axis.Value);
        Assert.Equal(BracketAxisValueClasses.ArgumentColon, axis.ValueWireClass);
    }
}
