namespace AIGuiders.Platform.Authoring.Core;

public sealed record AuthoringDiagnostic(
    AuthoringDiagnosticCode Code,
    string Message,
    int Line,
    int Column = 0,
    string? Section = null);

public enum AuthoringDiagnosticCode
{
    MissingCatalogHeader,
    MissingNotationDeclaration,
    NotationWireMismatch,
    MissingTableColumn,
    UnknownSection,
    DuplicateRow,
    InvalidSyntax,
}
