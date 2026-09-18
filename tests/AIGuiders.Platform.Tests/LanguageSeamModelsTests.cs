using AIGuiders.Platform.Execution.LanguageIntelligence;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class LanguageSeamModelsTests
{
    [Fact]
    public void RelationWire_and_legacy_AnchorWire_are_Execution_only()
    {
        var wire = new RelationWire("[Kind:CodeEdit; File:src/Foo.cs; Member:Bar]");
        Assert.Contains("Kind:CodeEdit", wire.Value);

        var legacy = new AnchorWire(wire.Value);
        Assert.Equal(wire.Value, legacy.ToRelationWire().Value);
    }

    [Fact]
    public void SniperScope_Empty_is_Execution_only_seam()
    {
        var scope = SniperScope.Empty();
        Assert.Null(scope.FromLine);
        Assert.Null(scope.Wire);
    }
}
