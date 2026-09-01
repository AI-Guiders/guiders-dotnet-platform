using AIGuiders.Platform.Authoring.Command.Catalog;
using AIGuiders.Platform.Authoring.Core;

namespace AIGuiders.Platform.Authoring.Conformance;

public static class CatalogConformance
{
    public static CatalogParseResult ValidateDocument(string text) =>
        CatalogParser.Parse(text);

    public static bool HasErrors(CatalogParseResult result) =>
        result.Diagnostics.Any(static d =>
            d.Code is AuthoringDiagnosticCode.InvalidSyntax
                or AuthoringDiagnosticCode.MissingCatalogHeader
                or AuthoringDiagnosticCode.MissingGrammarDeclaration
                or AuthoringDiagnosticCode.GrammarWireMismatch
                or AuthoringDiagnosticCode.UnknownGrammarId);
}
