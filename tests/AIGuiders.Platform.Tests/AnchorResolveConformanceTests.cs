#nullable enable
using AIGuiders.Platform.Execution.LanguageIntelligence.Anchors.Conformance;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class AnchorResolveConformanceTests
{
    [Fact]
    public void Anchor_resolve_spec_vectors_pass()
    {
        var json = ConformanceFixture.LoadEmbedded(
            "AIGuiders.Platform.Tests.Fixtures.LanguageIntelligence.anchor-resolve.spec.json");
        var spec = AnchorResolveSpecConformance.Load(json);
        Assert.Equal("anchor-resolve", spec.Surface);
        Assert.Empty(AnchorResolveSpecConformance.ValidateDocument(spec));
    }
}
