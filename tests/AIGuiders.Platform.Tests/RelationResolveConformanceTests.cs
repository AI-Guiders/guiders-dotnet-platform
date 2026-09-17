#nullable enable
using AIGuiders.Platform.Execution.LanguageIntelligence.Relations.Conformance;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class RelationResolveConformanceTests
{
    [Fact]
    public void Relation_resolve_spec_vectors_pass()
    {
        var json = ConformanceFixture.LoadEmbedded(
            "AIGuiders.Platform.Tests.Fixtures.LanguageIntelligence.anchor-resolve.spec.json");
        var spec = RelationResolveSpecConformance.Load(json);
        Assert.Equal("anchor-resolve", spec.Surface);
        Assert.Empty(RelationResolveSpecConformance.ValidateDocument(spec));
    }
}
