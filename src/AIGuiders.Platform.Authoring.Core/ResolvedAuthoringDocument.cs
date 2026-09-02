using AIGuiders.Platform.Modeling.Paths;

namespace AIGuiders.Platform.Authoring.Core;

public sealed class ResolvedAuthoringDocument
{
    ResolvedAuthoringDocument(AuthoringDocumentRef reference, string? text, string? displayPath)
    {
        Ref = reference;
        Text = text;
        DisplayPath = displayPath;
    }

    public AuthoringDocumentRef Ref { get; }

    /// <summary>Source text for logical files; null for federation imports resolved elsewhere.</summary>
    public string? Text { get; }

    /// <summary>Physical or logical path for diagnostics and LSP.</summary>
    public string? DisplayPath { get; }

    public static ResolvedAuthoringDocument LogicalFile(LogicalPath logical, string text, string? physicalPath) =>
        new(AuthoringDocumentRef.LogicalFile(logical), text, physicalPath ?? logical.Value);

    public static ResolvedAuthoringDocument FederationImport(string importPath) =>
        new(AuthoringDocumentRef.Federation(importPath), null, importPath);
}
