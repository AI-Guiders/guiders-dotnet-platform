#nullable enable

using System.Linq;
using AIGuiders.Platform.Authoring.Core;
using GdlAuthoring = AIGuiders.Platform.Modeling.Gdl.Authoring;
using GdlParseCatalog = AIGuiders.Platform.Modeling.Gdl.Parse.Catalog;
using Microsoft.FSharp.Core;

namespace AIGuiders.Platform.Authoring.Command.Catalog;

/// <summary>
/// GUIDERS-FSHARP-ADR-0003 §4.5 cutover: parse result SSOT via <see cref="ToModel"/> (Modeling.Gdl.Parse.Catalog).
/// </summary>
public sealed class CatalogParseResult
{
    public CatalogDocument? Document { get; init; }
    public IReadOnlyList<AuthoringDiagnostic> Diagnostics { get; init; } = [];
    public bool Success => Document is not null && Diagnostics.All(static d => d.Code != AuthoringDiagnosticCode.InvalidSyntax);

    public GdlParseCatalog.CatalogParseResult ToModel() => new(
        Document is null
            ? FSharpOption<GdlParseCatalog.CatalogDocument>.None
            : FSharpOption<GdlParseCatalog.CatalogDocument>.Some(Document.ToModel()),
        FSharpInterop.ToList(Diagnostics.Select(static d => d.ToModel())));

    public static CatalogParseResult FromModel(GdlParseCatalog.CatalogParseResult model) => new()
    {
        Document = FSharpOption<GdlParseCatalog.CatalogDocument>.get_IsSome(model.Document)
            ? CatalogDocument.FromModel(model.Document.Value)
            : null,
        Diagnostics = FSharpInterop.FromList(model.Diagnostics).Select(AuthoringDiagnosticExtensions.FromModel).ToList(),
    };
}

internal static class AuthoringDiagnosticExtensions
{
    internal static GdlAuthoring.AuthoringDiagnostic ToModel(this AuthoringDiagnostic diagnostic) =>
        GdlAuthoring.AuthoringDiagnosticModule.create(
            ToFsCode(diagnostic.Code),
            diagnostic.Message,
            diagnostic.Line);

    static GdlAuthoring.AuthoringDiagnosticCode ToFsCode(AuthoringDiagnosticCode code) => code switch
    {
        AuthoringDiagnosticCode.TopologyWireInvalid => GdlAuthoring.AuthoringDiagnosticCode.TopologyWireInvalid,
        AuthoringDiagnosticCode.MissingDeckHeader => GdlAuthoring.AuthoringDiagnosticCode.MissingDeckHeader,
        AuthoringDiagnosticCode.MissingCatalogHeader => GdlAuthoring.AuthoringDiagnosticCode.MissingCatalogHeader,
        AuthoringDiagnosticCode.MissingGrammarDeclaration => GdlAuthoring.AuthoringDiagnosticCode.MissingGrammarDeclaration,
        AuthoringDiagnosticCode.GrammarWireMismatch => GdlAuthoring.AuthoringDiagnosticCode.GrammarWireMismatch,
        AuthoringDiagnosticCode.UnknownGrammarId => GdlAuthoring.AuthoringDiagnosticCode.UnknownGrammarId,
        AuthoringDiagnosticCode.MissingTableColumn => GdlAuthoring.AuthoringDiagnosticCode.MissingTableColumn,
        AuthoringDiagnosticCode.UnknownSection => GdlAuthoring.AuthoringDiagnosticCode.UnknownSection,
        AuthoringDiagnosticCode.DuplicateRow => GdlAuthoring.AuthoringDiagnosticCode.DuplicateRow,
        AuthoringDiagnosticCode.InvalidSyntax => GdlAuthoring.AuthoringDiagnosticCode.InvalidSyntax,
        AuthoringDiagnosticCode.UnknownBundle => GdlAuthoring.AuthoringDiagnosticCode.UnknownBundle,
        AuthoringDiagnosticCode.UnknownProfile => GdlAuthoring.AuthoringDiagnosticCode.UnknownProfile,
        AuthoringDiagnosticCode.EntryFileNotFound => GdlAuthoring.AuthoringDiagnosticCode.EntryFileNotFound,
        AuthoringDiagnosticCode.EntryOutsideWorkspace => GdlAuthoring.AuthoringDiagnosticCode.EntryOutsideWorkspace,
        _ => GdlAuthoring.AuthoringDiagnosticCode.InvalidSyntax,
    };

    internal static AuthoringDiagnostic FromModel(GdlAuthoring.AuthoringDiagnostic diagnostic) =>
        CatalogInterop.FromDiagnostic(diagnostic);
}
