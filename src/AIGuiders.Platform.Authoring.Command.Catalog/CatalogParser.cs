using AIGuiders.Platform.Authoring.Command.Catalog.Parsing;
using AIGuiders.Platform.Authoring.Core;

namespace AIGuiders.Platform.Authoring.Command.Catalog;

public static class CatalogParser
{
    public static CatalogParseResult Parse(
        string text,
        string? sourcePath = null,
        ICatalogBundleLibrary? bundleLibrary = null) =>
        ParseLines(AuthoringSource.FromText(text), sourcePath, bundleLibrary);

    public static CatalogParseResult ParseFile(
        string path,
        ICatalogBundleLibrary? bundleLibrary = null) =>
        ParseLines(AuthoringSource.FromFile(path), path, bundleLibrary);

    internal static CatalogParseResult ParseLines(
        IReadOnlyList<AuthoringLine> lines,
        string? sourcePath,
        ICatalogBundleLibrary? bundleLibrary = null)
    {
        var context = new CatalogParseContext();
        CatalogDocumentWalkerFactory.Shared.Walk(lines, context);

        if (string.IsNullOrWhiteSpace(context.Planet))
        {
            context.Diagnostics.Add(new(AuthoringDiagnosticCode.MissingCatalogHeader, "Missing `catalog <planet>` header.", 1));
            return new() { Diagnostics = context.Diagnostics };
        }

        CatalogProfileResolver.Resolve(context, bundleLibrary);
        CatalogProfileResolver.ValidateCommandProfiles(context);

        var document = context.BuildDocument();
        CatalogGrammarValidator.Validate(document, context.Diagnostics);
        context.ValidateChannels();

        return new() { Document = document, Diagnostics = context.Diagnostics };
    }
}
