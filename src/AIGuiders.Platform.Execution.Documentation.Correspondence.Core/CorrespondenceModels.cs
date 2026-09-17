#nullable enable

using GdlCorrespondence = AIGuiders.Platform.Modeling.Documentation.Correspondence;
using Microsoft.FSharp.Core;

namespace AIGuiders.Platform.Execution.Documentation.Correspondence;

// GUIDERS-FSHARP-ADR-0003 §4.8 cutover: wire shapes SSOT in Modeling.Documentation.Correspondence.

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
    /// <summary>ADR obligation wire token "implements" (not TypeSystem implements-interface).</summary>
    public const string ImplementsObligation = GdlCorrespondence.Kind.ImplementsObligation;
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
    public GdlCorrespondence.AdrReference ToModel() => new() { Id = Id, Fragment = CorrespondenceFSharpInterop.ToFSharpOpt(Fragment) };
    public static AdrReference FromModel(GdlCorrespondence.AdrReference model) => new(
        model.Id,
        model.Fragment is not null && Microsoft.FSharp.Core.FSharpOption<string>.get_IsSome(model.Fragment) ? model.Fragment.Value : null);
}

public sealed record ForwardDoc(string Path, string Title, string? Abs = null, string? Kind = null)
{
    public GdlCorrespondence.ForwardDoc ToModel() => new()
    {
        Path = Path,
        Title = Title,
        Abs = CorrespondenceFSharpInterop.ToFSharpOpt(Abs),
        Kind = CorrespondenceFSharpInterop.ToFSharpOpt(Kind),
    };
    public static ForwardDoc FromModel(GdlCorrespondence.ForwardDoc model) => new(
        model.Path,
        model.Title,
        model.Abs is not null && Microsoft.FSharp.Core.FSharpOption<string>.get_IsSome(model.Abs) ? model.Abs.Value : null,
        model.Kind is not null && Microsoft.FSharp.Core.FSharpOption<string>.get_IsSome(model.Kind) ? model.Kind.Value : null);
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
        MemberKey = CorrespondenceFSharpInterop.ToFSharpOpt(MemberKey),
        Wire = Wire,
        DocLineHint = DocLineHint,
        Excerpt = CorrespondenceFSharpInterop.ToFSharpOpt(Excerpt),
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
        MemberKey = CorrespondenceFSharpInterop.ToFSharpOpt(MemberKey),
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
        FileRel = CorrespondenceFSharpInterop.ToFSharpOpt(FileRel),
        FeatureLine = CorrespondenceFSharpInterop.ToFSharpOpt(FeatureLine),
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
        FeatureLine = CorrespondenceFSharpInterop.ToFSharpOpt(FeatureLine),
        FeatureDocs = FeatureDocs,
        AdrLine = AdrLine,
        DocPaths = Microsoft.FSharp.Collections.ListModule.OfSeq(DocPaths),
        ForwardDocs = ForwardDocs.Select(d => d.ToModel()).ToArray(),
    };
}

internal static class CorrespondenceFSharpInterop
{
    internal static FSharpOption<string> ToFSharpOpt(string? value) =>
        string.IsNullOrEmpty(value) ? FSharpOption<string>.None : FSharpOption<string>.Some(value!);
}
