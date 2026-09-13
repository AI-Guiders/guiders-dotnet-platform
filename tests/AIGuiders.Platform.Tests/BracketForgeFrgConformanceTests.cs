#nullable enable
using AIGuiders.Platform.Modeling.Notations.Bracket;
using AIGuiders.Platform.Notations.Bracket.Conformance;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class BracketForgeFrgConformanceTests
{
    [Fact]
    public void Bracket_forge_frg_spec_vectors_pass()
    {
        var json = ConformanceFixture.LoadEmbedded("AIGuiders.Platform.Tests.Fixtures.Notation.bracket-forge-frg.spec.json");
        var spec = BracketSpecConformance.Load(json);
        Assert.Equal("bracket-forge-frg", spec.Surface);
        Assert.Empty(BracketSpecConformance.ValidateDocument(
            spec,
            BracketProfiles.ForgeFrg,
            BracketAxisValuePlans.ForgeFrgCompound));
    }
}
