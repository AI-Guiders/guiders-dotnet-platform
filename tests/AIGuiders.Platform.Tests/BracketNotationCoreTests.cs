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
        Assert.Equal(BracketAxisShape.KeyValue, profile.AxisShape);
        Assert.True(profile.StripOuterTerminals);
        Assert.True(profile.RespectBracketDepthOnAxisSplit);
        Assert.Contains("Anchor", profile.NestedAxisKeys!);
    }

    [Fact]
    public void BracketAxis_supports_nested_wire_slot()
    {
        var nested = new NormalizedBracketWire(
            BracketProfiles.CdpSquareKeyValue.Id,
            [new BracketAxis("F", "x.cs")],
            "[F:x.cs]");
        var wire = new NormalizedBracketWire(
            BracketProfiles.CdpSquareKeyValue.Id,
            [new BracketAxis("Anchor", "[F:x.cs]", nested)],
            "[Anchor:[F:x.cs]]");
        Assert.NotNull(wire.Axes[0].Nested);
        Assert.Equal("x.cs", wire.Axes[0].Nested!.Axes[0].Value);
    }

    [Fact]
    public void Value_may_contain_pair_delimiter_after_first_colon()
    {
        // CDP: K:Parameter:name, S:if:2 — only first ':' splits key from value.
        var axis = new BracketAxis("K", "Parameter:Run");
        Assert.Equal("Parameter:Run", axis.Value);
    }
}
