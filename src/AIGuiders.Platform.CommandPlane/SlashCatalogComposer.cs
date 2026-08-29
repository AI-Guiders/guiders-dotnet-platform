#nullable enable

namespace AIGuiders.Platform.CommandPlane;

/// <summary>Merges multiple command sources into one slash catalog index.</summary>
public static class SlashCatalogComposer
{
    public static SlashCatalogIndex Build(params ICommandSource[] sources) =>
        Build((IEnumerable<ICommandSource>)sources);

    public static SlashCatalogIndex Build(IEnumerable<ICommandSource> sources)
    {
        SlashCatalogIndex index = SlashCatalogIndex.Empty;
        foreach (var source in sources)
        {
            index = index.Merge(SlashCatalogIndex.FromDescriptors(source.Load()));
        }

        return index;
    }
}
