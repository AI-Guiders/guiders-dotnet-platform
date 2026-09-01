namespace AIGuiders.Platform.Authoring.Core;

public sealed record AuthoringDiagnostic(
    AuthoringDiagnosticCode Code,
    string Message,
    int Line,
    int Column = 0,
    string? Section = null);

public enum AuthoringDiagnosticCode
{
    MissingDeckHeader,
    MissingCatalogHeader,
    MissingGrammarDeclaration,
    GrammarWireMismatch,
    UnknownGrammarId,
    MissingTableColumn,
    UnknownSection,
    DuplicateRow,
    InvalidSyntax,
    UnknownBundle,
    UnknownProfile,
    EntryFileNotFound,
    EntryOutsideWorkspace,
}
