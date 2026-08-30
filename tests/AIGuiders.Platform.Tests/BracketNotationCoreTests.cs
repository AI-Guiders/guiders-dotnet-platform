#nullable enable
using AIGuiders.Platform.Notations.Bracket;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class BracketNotationCoreTests
{
    [Fact]
    public void SquareKeyValue_profile_uses_default_delimiters()
    {
        var profile = BracketProfiles.SquareKeyValue;
        Assert.Equal("[", profile.StartTerminal);
        Assert.Equal("]", profile.EndTerminal);
        Assert.Equal(';', profile.AxisSeparator);
        Assert.Equal(':', profile.PairDelimiter);
        Assert.Equal(BracketAxisShape.KeyValue, profile.AxisShape);
    }

    [Fact]
    public void NormalizedBracketWire_carries_axes_and_profile()
    {
        var wire = new NormalizedBracketWire(
            BracketProfiles.SquareKeyValue.Id,
            [new BracketAxis("F", "Program.cs"), new BracketAxis("M", "Foo")],
            "[F:Program.cs;M:Foo]");
        Assert.Equal(2, wire.Axes.Count);
        Assert.Equal("F", wire.Axes[0].Key);
        Assert.Equal("Program.cs", wire.Axes[0].Value);
    }
}
