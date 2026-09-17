#nullable enable

using AIGuiders.Platform.Execution.LanguageIntelligence.Relations;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class RelationSeamContractTests
{
    [Fact]
    public void Resolve_and_materialize_seams_are_declared()
    {
        Assert.True(typeof(IResolveRelation).IsInterface);
        Assert.True(typeof(IMaterializeRelation).IsInterface);
        Assert.NotNull(UnresolvedRelationSeams.NotRegistered);
    }
}
