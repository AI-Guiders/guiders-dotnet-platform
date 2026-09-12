#nullable enable

using GdlCorrespondence = AIGuiders.Platform.Modeling.Gdl.Correspondence;

namespace AIGuiders.Platform.Execution.Documentation.Correspondence;

// GUIDERS-FSHARP-ADR-0003 §4.8 cutover: wire shapes SSOT in Modeling.Gdl.Correspondence.

public static class CorrespondenceSchema
{
    public const string V0 = GdlCorrespondence.Schema.V0;
}

public static class CorrespondenceProvenance
{
    public const string Bracket = GdlCorrespondence.Provenance.Bracket;
    public const string DocBody = GdlCorrespondence.Provenance.DocBody;
    public const string WorkspaceToml = GdlCorrespondence.Provenance.WorkspaceToml;
}

public static class CorrespondenceKind
{
    public const string Documents = GdlCorrespondence.Kind.Documents;
    public const string Implements = GdlCorrespondence.Kind.Implements;
    public const string Related = GdlCorrespondence.Kind.Related;
    public const string Constrains = GdlCorrespondence.Kind.Constrains;
    public const string Normates = GdlCorrespondence.Kind.Normates;
    public const string VerifiedBy = GdlCorrespondence.Kind.VerifiedBy;
}

public static class AdrLifecycleTag
{
    public const string Proposed = GdlCorrespondence.AdrLifecycleTag.Proposed;
    public const string Accepted = GdlCorrespondence.AdrLifecycleTag.Accepted;
    public const string Implemented = GdlCorrespondence.AdrLifecycleTag.Implemented;
    public const string Superseded = GdlCorrespondence.AdrLifecycleTag.Superseded;
    public const string Deprecated = GdlCorrespondence.AdrLifecycleTag.Deprecated;
}

public sealed record AdrReference(string Id, string? Fragment = null)
{
    public GdlCorrespondence.AdrReference ToModel() => new() { Id = Id, Fragment = Fragment };
    public static AdrReference FromModel(GdlCorrespondence.AdrReference model) => new(
        model.Id,
        Microsoft.FSharp.Core.FSharpOption<string>.get_IsSome(model.Fragment) ? model.Fragment.Value : null);
}

public sealed record ForwardDoc(string Path, string Title, string? Abs = null, string? Kind = null)
{
    public GdlCorrespondence.ForwardDoc ToModel() => new() { Path = Path, Title = Title, Abs = Abs, Kind = Kind };
    public static ForwardDoc FromModel(GdlCorrespondence.ForwardDoc model) => new(
        model.Path,
        model.Title,
        Microsoft.FSharp.Core.FSharpOption<string>.get_IsSome(model.Abs) ? model.Abs.Value : null,
        Microsoft.FSharp.Core.FSharpOption<string>.get_IsSome(model.Kind) ? model.Kind.Value : null);
}

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
    string? Excerpt = null)
{
    public GdlCorrespondence.ReverseAnchor ToModel() => new()
    {
        DocPath = DocPath,
        DocTitle = DocTitle,
        Provenance = Provenance,
        Kind = Kind,
        File = File,
        LineStart = LineStart,
        LineEnd = LineEnd,
        MemberKey = MemberKey,
        Wire = Wire,
        DocLineHint = DocLineHint,
        Excerpt = Excerpt,
    };
}

public sealed record ExplicitCodeAnchor(
    string DocPath,
    string File,
    int? LineStart,
    int? LineEnd,
    string? MemberKey,
    string Provenance,
    string Kind,
    string DefaultKind = "documents")
{
    public GdlCorrespondence.ExplicitCodeAnchor ToModel() => new()
    {
        DocPath = DocPath,
        File = File,
        LineStart = LineStart,
        LineEnd = LineEnd,
        MemberKey = MemberKey,
        Provenance = Provenance,
        Kind = Kind,
        DefaultKind = DefaultKind,
    };
}

public sealed record CorrespondenceResult(
    string WorkspaceRoot,
    string? FileRel,
    string? FeatureLine,
    string[] FeatureDocs,
    string AdrLine,
    ForwardDoc[] ForwardDocs,
    ReverseAnchor[] ReverseAnchors,
    string[] ActiveLayers,
    string TomlPath)
{
    public GdlCorrespondence.CorrespondenceResult ToModel() => new()
    {
        WorkspaceRoot = WorkspaceRoot,
        FileRel = FileRel,
        FeatureLine = FeatureLine,
        FeatureDocs = FeatureDocs,
        AdrLine = AdrLine,
        ForwardDocs = ForwardDocs.Select(d => d.ToModel()).ToArray(),
        ReverseAnchors = ReverseAnchors.Select(a => a.ToModel()).ToArray(),
        ActiveLayers = ActiveLayers,
        TomlPath = TomlPath,
    };
}

public sealed record ForwardMapResult(
    string? FeatureLine,
    string[] FeatureDocs,
    string AdrLine,
    IReadOnlyList<string> DocPaths,
    ForwardDoc[] ForwardDocs)
{
    public GdlCorrespondence.ForwardMapResult ToModel() => new()
    {
        FeatureLine = FeatureLine,
        FeatureDocs = FeatureDocs,
        AdrLine = AdrLine,
        DocPaths = Microsoft.FSharp.Collections.ListModule.OfSeq(DocPaths),
        ForwardDocs = ForwardDocs.Select(d => d.ToModel()).ToArray(),
    };
}
