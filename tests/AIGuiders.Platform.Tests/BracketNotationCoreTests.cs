#nullable enable
using AIGuiders.Platform.Notations.Bracket;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class BracketNotationCoreTests
{
    [Fact]
    public void NormalizedBracketWire_carries_pair_and_inner()
    {
        var wire = new NormalizedBracketWire(BracketPairKind.Angle, "C-k", Raw: "<C-k>");
        Assert.Equal(BracketPairKind.Angle, wire.Pair);
        Assert.Equal("C-k", wire.Inner);
        Assert.Equal("<C-k>", wire.Raw);
    }
}
