using AIGuiders.Platform.Authoring.Command.Catalog.Parsing;
using AIGuiders.Platform.Authoring.Core;

namespace AIGuiders.Platform.Authoring.Command.Catalog;

public static class CatalogParser
{
    public static CatalogParseResult Parse(string text, string? sourcePath = null) =>
        ParseLines(AuthoringSource.FromText(text), sourcePath);

    public static CatalogParseResult ParseFile(string path) =>
        ParseLines(AuthoringSource.FromFile(path), path);

    internal static CatalogParseResult ParseLines(IReadOnlyList<AuthoringLine> lines, string? sourcePath)
    {
        var context = new CatalogParseContext();
        CatalogDocumentWalkerFactory.Shared.Walk(lines, context);

        if (string.IsNullOrWhiteSpace(context.Planet))
        {
            context.Diagnostics.Add(new(AuthoringDiagnosticCode.MissingCatalogHeader, "Missing `catalog <planet>` header.", 1));
            return new() { Diagnostics = context.Diagnostics };
        }

        var document = context.BuildDocument();
        CatalogGrammarValidator.Validate(document, context.Diagnostics);
        context.ValidateChannels();

        return new() { Document = document, Diagnostics = context.Diagnostics };
    }
}
