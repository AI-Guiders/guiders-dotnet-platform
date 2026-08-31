#nullable enable

namespace AIGuiders.Platform.CommandPlane;

/// <summary>Merges multiple command sources into one slash catalog index.</summary>
public static class CommandCatalogComposer
{
    public static CommandCatalogIndex Build(params ICommandSource[] sources) =>
        Combinations.Catalog.CommandCatalogCombination.Compose(sources);

    public static CommandCatalogIndex Build(IEnumerable<ICommandSource> sources) =>
        Combinations.Catalog.CommandCatalogCombination.Compose(sources);
}
