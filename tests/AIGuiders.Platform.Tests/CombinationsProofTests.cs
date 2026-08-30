using AIGuiders.Platform.Tools.CombinationsProof;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class CombinationsProofTests
{
    [Fact]
    public void ShipFirst_Z3_proofs_hold()
    {
        Assert.True(ShipFirstProof.ProveBaselineWinsOnCollision());
        Assert.True(ShipFirstProof.ProveOverlayFillsMissingKeys());
    }
}
