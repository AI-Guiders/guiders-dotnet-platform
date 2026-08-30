#nullable enable

namespace AIGuiders.Platform.CommandPlane;

/// <summary>Merges multiple command sources into one slash catalog index.</summary>
public static class SlashCatalogComposer
{
    public static SlashCatalogIndex Build(params ICommandSource[] sources) =>
        Combinations.Slash.SlashCatalogCombination.Compose(sources);

    public static SlashCatalogIndex Build(IEnumerable<ICommandSource> sources) =>
        Combinations.Slash.SlashCatalogCombination.Compose(sources);
}
