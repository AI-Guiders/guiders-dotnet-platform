namespace AIGuiders.Platform.Authoring.Command.Catalog.Parsing;

public interface ICatalogSectionHandler
{
    string Keyword { get; }

    void Apply(CatalogParseContext context, CatalogSectionBlock block);
}
