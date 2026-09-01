using AIGuiders.Platform.Authoring.Core;

namespace AIGuiders.Platform.Authoring.Command.Catalog.Parsing;

public sealed record CatalogSectionBlock(
    string Keyword,
    AuthoringSurfaceKind SurfaceKind,
    IReadOnlyList<AuthoringLine> Body);
