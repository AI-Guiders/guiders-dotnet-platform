using AIGuiders.Platform.Authoring.Command.Bundles;
using AIGuiders.Platform.Authoring.Command.Catalog;
using AIGuiders.Platform.Authoring.Core;
using AIGuiders.Platform.Authoring.Emit;

namespace AIGuiders.Platform.Execution.CommandPlane.Catalog.CodeGen;

public sealed class CatalogQuarryPlugin : IGdlQuarryPlugin
{
    public string QuarryId => GdlQuarryIds.Catalog;

    public IReadOnlyList<string> SupportedLanguages { get; } = ["cs"];

    public string? SurfaceId => null;

    public bool CanHandle(string quarryId, string lang, string? surface) =>
        string.Equals(quarryId, GdlQuarryIds.Catalog, StringComparison.OrdinalIgnoreCase)
        && string.Equals(lang, "cs", StringComparison.OrdinalIgnoreCase);

    public GdlEmitResult Emit(GdlEmitRequest request)
    {
        var open = OpenCatalog(request.Path, request.WorkspaceRoot);
        if (open.Document is null)
        {
            return new()
            {
                Success = false,
                Diagnostics = MapDiagnostics(open.Diagnostics),
            };
        }

        return new()
        {
            Success = true,
            GeneratedCode = CatalogCatalogEmitter.EmitCSharp(
                open.Document,
                request.Namespace,
                request.ClassName),
        };
    }

    public GdlValidateResult Validate(GdlValidateRequest request)
    {
        var open = OpenCatalog(request.Path, request.WorkspaceRoot);
        if (open.Document is null)
        {
            return new()
            {
                Success = false,
                Diagnostics = MapDiagnostics(open.Diagnostics),
            };
        }

        var diagnostics = MapDiagnostics(open.Diagnostics);
        if (HasFatalDiagnostics(open.Diagnostics))
        {
            return new() { Success = false, Diagnostics = diagnostics };
        }

        var importCount = open.Project?.Documents.Count(static d => d.Ref.Kind == AuthoringDocumentKind.FederationImport) ?? 0;
        return new()
        {
            Success = true,
            Summary = $"validate: ok — {CatalogSummary.Format(open.Document)} (project docs: {open.Project?.Documents.Count ?? 0}, wire imports: {importCount})",
            Diagnostics = diagnostics,
        };
    }

    public GdlSatResult Sat(GdlSatRequest request) =>
        GdlSatResult.NotSupported("sat: skipped — catalog quarry has no sat observers (see config quarry ADR-0064)");

    private static CatalogProjectResult OpenCatalog(string catalogPath, string? workspaceRoot) =>
        CatalogProject.Open(
            ResolveWorkspaceRoot(catalogPath, workspaceRoot),
            catalogPath,
            CatalogBundleLibrary.Federation);

    private static bool HasFatalDiagnostics(IReadOnlyList<AuthoringDiagnostic> diagnostics) =>
        diagnostics.Any(static d =>
            d.Code is AuthoringDiagnosticCode.GrammarWireMismatch
                or AuthoringDiagnosticCode.MissingCatalogHeader
                or AuthoringDiagnosticCode.MissingGrammarDeclaration
                or AuthoringDiagnosticCode.UnknownGrammarId
                or AuthoringDiagnosticCode.UnknownBundle
                or AuthoringDiagnosticCode.UnknownProfile
                or AuthoringDiagnosticCode.InvalidSyntax
                or AuthoringDiagnosticCode.EntryFileNotFound
                or AuthoringDiagnosticCode.EntryOutsideWorkspace);

    private static IReadOnlyList<GdlDiagnostic> MapDiagnostics(IEnumerable<AuthoringDiagnostic> diagnostics) =>
        diagnostics.Select(static d => new GdlDiagnostic(d.Code.ToString(), d.Message, d.Line)).ToArray();

    private static string ResolveWorkspaceRoot(string catalogPath, string? explicitRoot)
    {
        if (!string.IsNullOrWhiteSpace(explicitRoot))
        {
            return Path.GetFullPath(explicitRoot);
        }

        var dir = new DirectoryInfo(Path.GetDirectoryName(Path.GetFullPath(catalogPath))!);
        while (dir is not null)
        {
            if (dir.GetFiles("*.slnx").Length > 0
                || dir.GetFiles("*.sln").Length > 0
                || Directory.Exists(Path.Combine(dir.FullName, ".git")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        return Path.GetDirectoryName(Path.GetFullPath(catalogPath))!;
    }
}
