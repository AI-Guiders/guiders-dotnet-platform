#nullable enable

namespace AIGuiders.Platform.Execution.Documentation.Correspondence;

public static class CorrespondenceSchema
{
    public const string V0 = "correspondence/v0";
}

public static class CorrespondenceProvenance
{
    public const string Bracket = "bracket";
    public const string DocBody = "doc_body";
    public const string WorkspaceToml = "workspace_toml";
}

public sealed record ForwardDoc(string Path, string Title);

public sealed record ReverseAnchor(
    string DocPath,
    string DocTitle,
    string Provenance,
    string Kind,
    string File,
    int? LineStart,
    int? LineEnd,
    string? MemberKey,
    string Wire,
    int? DocLineHint = null,
    string? Excerpt = null);

public sealed record ExplicitCodeAnchor(
    string DocPath,
    string File,
    int? LineStart,
    int? LineEnd,
    string? MemberKey,
    string Provenance,
    string Kind,
    string DefaultKind = "documents");

public sealed record CorrespondenceResult(
    string WorkspaceRoot,
    string? FileRel,
    string? FeatureLine,
    string[] FeatureDocs,
    string AdrLine,
    ForwardDoc[] ForwardDocs,
    ReverseAnchor[] ReverseAnchors,
    string[] ActiveLayers,
    string TomlPath);

public sealed record ForwardMapResult(
    string? FeatureLine,
    string[] FeatureDocs,
    string AdrLine,
    IReadOnlyList<string> DocPaths,
    ForwardDoc[] ForwardDocs);
