using AIGuiders.Platform.Authoring.Core;

namespace AIGuiders.Platform.Authoring.Command.Catalog;

public static class CatalogProject
{
    public static CatalogProjectResult Open(
        string workspaceRoot,
        string catalogFilePath,
        ICatalogBundleLibrary? bundleLibrary = null)
    {
        var load = AuthoringProjectLoader.OpenSingleFile(workspaceRoot, catalogFilePath);
        if (load.Project is null)
        {
            return new() { Diagnostics = load.Diagnostics };
        }

        var entry = load.Project.Documents[0];
        var parse = CatalogParser.Parse(entry.Text!, entry.DisplayPath, bundleLibrary);
        var diagnostics = load.Diagnostics.Concat(parse.Diagnostics).ToList();

        if (parse.Document is null)
        {
            return new() { Project = load.Project, Diagnostics = diagnostics };
        }

        var documents = new List<ResolvedAuthoringDocument> { entry };
        foreach (var import in parse.Document.Imports)
        {
            documents.Add(ResolvedAuthoringDocument.FederationImport(import));
        }

        var project = load.Project.WithDocuments(documents);
        return new()
        {
            Project = project,
            Document = parse.Document,
            Diagnostics = diagnostics,
        };
    }
}

public sealed class CatalogProjectResult
{
    public AuthoringProject? Project { get; init; }

    public CatalogDocument? Document { get; init; }

    public IReadOnlyList<AuthoringDiagnostic> Diagnostics { get; init; } = [];

    public bool Success => Document is not null
        && Diagnostics.All(static d => d.Code != AuthoringDiagnosticCode.InvalidSyntax);
}
