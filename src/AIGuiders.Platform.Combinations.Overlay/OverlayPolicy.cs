#nullable enable

namespace AIGuiders.Platform.Combinations.Overlay;

/// <summary>Named overlay policy: readable recipe bound to a <see cref="Combinator{T}"/>.</summary>
public sealed class OverlayPolicy<T>(
    string name,
    CombinationSemantics semantics,
    Combinator<T> combinator)
{
    public string Name { get; } = name;
    public CombinationSemantics Semantics { get; } = semantics;
    public Combinator<T> Combinator { get; } = combinator;

    public static implicit operator Combinator<T>(OverlayPolicy<T> policy) => policy.Combinator;
}
