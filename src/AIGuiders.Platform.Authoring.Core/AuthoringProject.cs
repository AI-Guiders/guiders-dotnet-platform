using AIGuiders.Platform.Modeling.Paths;

namespace AIGuiders.Platform.Authoring.Core;

public sealed class AuthoringProject
{
    public AuthoringProject(
        string workspaceRoot,
        LogicalPath entry,
        IReadOnlyList<ResolvedAuthoringDocument> documents)
    {
        WorkspaceRoot = workspaceRoot;
        Entry = entry;
        Documents = documents;
    }

    /// <summary>Physical workspace root for logical ↔ physical mapping.</summary>
    public string WorkspaceRoot { get; }

    public LogicalPath Entry { get; }

    public IReadOnlyList<ResolvedAuthoringDocument> Documents { get; }

    public AuthoringProject WithDocuments(IReadOnlyList<ResolvedAuthoringDocument> documents) =>
        new(WorkspaceRoot, Entry, documents);
}
