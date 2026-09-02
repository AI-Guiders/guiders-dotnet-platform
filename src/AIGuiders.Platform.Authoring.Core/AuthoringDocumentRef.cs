using AIGuiders.Platform.Modeling.Paths;

namespace AIGuiders.Platform.Authoring.Core;

public readonly record struct AuthoringDocumentRef(AuthoringDocumentKind Kind, string Path)
{
    public static AuthoringDocumentRef LogicalFile(LogicalPath path) =>
        new(AuthoringDocumentKind.LogicalFile, path.Value);

    public static AuthoringDocumentRef Federation(string wirePath) =>
        new(AuthoringDocumentKind.FederationImport, wirePath);

    public bool IsLogicalFile => Kind == AuthoringDocumentKind.LogicalFile;

    public bool IsFederationImport => Kind == AuthoringDocumentKind.FederationImport;
}
