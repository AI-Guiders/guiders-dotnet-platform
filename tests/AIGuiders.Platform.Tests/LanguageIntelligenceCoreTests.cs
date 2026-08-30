#nullable enable
using AIGuiders.Platform.LanguageIntelligence;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class LanguageIntelligenceCoreTests
{
    [Fact]
    public void BufferEditOutcome_carries_selection()
    {
        var outcome = BufferEditOutcome.FromText("abc", 1, 2);
        Assert.Equal("abc", outcome.Text);
        Assert.Equal(1, outcome.SelectionStart);
        Assert.Equal(2, outcome.SelectionEnd);
    }

    [Fact]
    public void Locus_defaults_to_text_tier()
    {
        var locus = new Locus(0, 5);
        Assert.Equal(ResolveTier.Text, locus.Tier);
    }
}
