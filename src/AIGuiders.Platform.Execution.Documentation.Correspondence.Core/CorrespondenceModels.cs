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

public static class CorrespondenceKind
{
    public const string Documents = "documents";
    public const string Implements = "implements";
    public const string Related = "related";
    public const string Constrains = "constrains";
    /// <summary>ADR/axiom normatively constrains code or graph scope (GUIDERS-FSHARP-ADR-0006).</summary>
    public const string Normates = "normates";
    /// <summary>ADR facts block satisfied by golden session or CI evidence.</summary>
    public const string VerifiedBy = "verified_by";
}

public static class AdrLifecycleTag
{
    public const string Proposed = "proposed";
    public const string Accepted = "accepted";
    public const string Implemented = "implemented";
    public const string Superseded = "superseded";
    public const string Deprecated = "deprecated";
}

public sealed record AdrReference(string Id, string? Fragment = null);

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
