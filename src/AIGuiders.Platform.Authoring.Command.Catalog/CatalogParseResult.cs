using AIGuiders.Platform.Authoring.Core;

namespace AIGuiders.Platform.Authoring.Command.Catalog;

public sealed class CatalogParseResult
{
    public CatalogDocument? Document { get; init; }
    public IReadOnlyList<AuthoringDiagnostic> Diagnostics { get; init; } = [];
    public bool Success => Document is not null && Diagnostics.All(static d => d.Code != AuthoringDiagnosticCode.InvalidSyntax);
}
