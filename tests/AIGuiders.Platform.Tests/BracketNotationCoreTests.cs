#nullable enable
using AIGuiders.Platform.Notations.Bracket;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class BracketNotationCoreTests
{
    [Fact]
    public void NotationKvPair_splits_on_first_sign_only()
    {
        Assert.True(NotationKvPair.TrySplitFirst("S:for:2", ':', out var kv, out _));
        Assert.Equal("S", kv.Key);
        Assert.Equal(':', kv.Sign);
        Assert.Equal("for:2", kv.Value);

        Assert.True(NotationKvPair.TrySplitFirst("doc=README.md", '=', out var console, out _));
        Assert.Equal("doc", console.Key);
        Assert.Equal("README.md", console.Value);
    }

    [Fact]
    public void Bracket_profile_uses_colon_kv_sign_and_semicolon_list()
    {
        var profile = BracketProfiles.CdpSquareKeyValue;
        Assert.Equal("[", profile.StartTerminal);
        Assert.Equal(':', profile.KvSign);
        Assert.Equal(';', profile.ListSeparator);
    }

    [Fact]
    public void Inner_scope_value_is_kv_with_same_sign()
    {
        Assert.True(NotationKvPair.TrySplitFirst("for:2", ':', out var scope, out _));
        Assert.Equal("for", scope.Key);
        Assert.Equal("2", scope.Value);
        Assert.Equal(
            BracketAxisValueClasses.Kv,
            BracketAxisValuePlans.CdpCode.ByAxisKey["S"]);
    }

    [Fact]
    public void BracketAxis_is_envelope_kv()
    {
        var axis = new BracketAxis("F", ':', "src/Foo.cs");
        Assert.Equal(new NotationKvPair("F", ':', "src/Foo.cs"), axis.ToKvPair());
    }
}
