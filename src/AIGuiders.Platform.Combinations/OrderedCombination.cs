#nullable enable

namespace AIGuiders.Platform.Combinations;

/// <summary>Ordered fold over materialized layers (GUIDERS-ADR-0030).</summary>
public static class OrderedCombination
{
    public static T Fold<T>(IReadOnlyList<T> layers, Combinator<T> combiner)
    {
        ArgumentNullException.ThrowIfNull(layers);
        if (layers.Count == 0)
            throw new ArgumentException("At least one layer is required.", nameof(layers));
        ArgumentNullException.ThrowIfNull(combiner);

        var acc = layers[0];
        for (var i = 1; i < layers.Count; i++)
            acc = combiner(acc, layers[i]);
        return acc;
    }

    /// <summary>Projects each layer to an accumulator and folds with <paramref name="combiner"/>.</summary>
    public static TAccum FoldLayers<TLayer, TAccum>(
        IEnumerable<TLayer> layers,
        TAccum seed,
        Func<TLayer, TAccum> project,
        Combinator<TAccum> combiner)
    {
        ArgumentNullException.ThrowIfNull(layers);
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(combiner);

        var acc = seed;
        foreach (var layer in layers)
            acc = combiner(acc, project(layer));
        return acc;
    }
}
