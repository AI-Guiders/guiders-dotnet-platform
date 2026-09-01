using AIGuiders.Platform.Paths;

namespace AIGuiders.Platform.Authoring.Core;

public static class AuthoringProjectGraph
{
    public delegate IEnumerable<string> ReferencePathResolver(string baseDirectory, string reference);

    public static AuthoringProject ExpandLogicalImports(
        AuthoringProject project,
        ReferencePathResolver resolveReferencePaths)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(resolveReferencePaths);

        if (project.Documents.Count == 0)
        {
            return project;
        }

        var documents = new List<ResolvedAuthoringDocument>();
        var visitedPhysical = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var wireRefs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void AddWire(string wirePath)
        {
            if (!wireRefs.Add(wirePath))
            {
                return;
            }

            documents.Add(ResolvedAuthoringDocument.FederationImport(wirePath));
        }

        void Walk(ResolvedAuthoringDocument document, string baseDirectory)
        {
            if (document.Text is null)
            {
                return;
            }

            foreach (var directive in AuthoringImportGraph.ScanText(document.Text))
            {
                if (directive.TargetKind == AuthoringImportTargetKind.WireLibrary)
                {
                    AddWire(directive.Path);
                    continue;
                }

                foreach (var physical in resolveReferencePaths(baseDirectory, directive.Path))
                {
                    if (!visitedPhysical.Add(physical) || !File.Exists(physical))
                    {
                        continue;
                    }

                    var logical = PathBoundary.ToLogical(project.WorkspaceRoot, physical);
                    if (logical is null)
                    {
                        continue;
                    }

                    var text = File.ReadAllText(physical);
                    var child = ResolvedAuthoringDocument.LogicalFile(logical.Value, text, physical);
                    documents.Add(child);
                    Walk(child, Path.GetDirectoryName(physical) ?? baseDirectory);
                }
            }
        }

        var entry = project.Documents[0];
        documents.Add(entry);
        if (entry.DisplayPath is not null)
        {
            visitedPhysical.Add(entry.DisplayPath);
        }

        var baseDir = Path.GetDirectoryName(entry.DisplayPath ?? project.WorkspaceRoot) ?? project.WorkspaceRoot;
        Walk(entry, baseDir);

        return project.WithDocuments(documents);
    }
}
