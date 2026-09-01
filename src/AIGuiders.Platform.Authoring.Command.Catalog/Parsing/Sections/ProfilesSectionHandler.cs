using AIGuiders.Platform.Authoring.Core;

namespace AIGuiders.Platform.Authoring.Command.Catalog.Parsing.Sections;

public sealed class ProfilesSectionHandler : IAuthoringSectionHandler<CatalogParseContext>
{
    public string Keyword => "profiles";

    public void Apply(CatalogParseContext context, AuthoringSectionBlock block)
    {
        IReadOnlyList<Dictionary<string, string>> maps = block.SurfaceKind == AuthoringSurfaceKind.Table
            ? TableSurface.ParseMaps(block.Body)
            : KvDesugar.ProfileRows(block.Body);

        CatalogProfileTable.MergeRows(context.Profiles, maps);
    }
}
