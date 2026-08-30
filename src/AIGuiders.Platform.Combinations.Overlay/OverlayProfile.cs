#nullable enable

namespace AIGuiders.Platform.Combinations.Overlay;

public static class OverlayProfile
{
    public static OverlayProfileBuilder<T> For<T>(string name, CombinationSemantics semantics)
        where T : class =>
        new(name, semantics);
}
