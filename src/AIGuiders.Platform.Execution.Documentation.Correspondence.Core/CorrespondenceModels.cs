#nullable enable

using GdlCorrespondence = AIGuiders.Platform.Modeling.Documentation.Correspondence;
using Microsoft.FSharp.Collections;
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
    public GdlCorrespondence.AdrReference ToModel() =>
        new(Id, CorrespondenceFSharpInterop.ToFSharpOpt(Fragment));

    public static AdrReference FromModel(GdlCorrespondence.AdrReference model) => new(
        model.Id,
        model.Fragment is not null && FSharpOption<string>.get_IsSome(model.Fragment) ? model.Fragment.Value : null);
}

public sealed record ForwardDoc(string Path, string Title, string? Abs = null, string? Kind = null)
{
    public GdlCorrespondence.ForwardDoc ToModel() =>
        new(
            Path,
            Title,
            CorrespondenceFSharpInterop.ToFSharpOpt(Abs),
            CorrespondenceFSharpInterop.ToFSharpOpt(Kind));

    public static ForwardDoc FromModel(GdlCorrespondence.ForwardDoc model) => new(
        model.Path,
        model.Title,
        model.Abs is not null && FSharpOption<string>.get_IsSome(model.Abs) ? model.Abs.Value : null,
        model.Kind is not null && FSharpOption<string>.get_IsSome(model.Kind) ? model.Kind.Value : null);
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
    public GdlCorrespondence.ReverseAnchor ToModel() =>
        new(
            DocPath,
            DocTitle,
            Provenance,
            Kind,
            File,
            CorrespondenceFSharpInterop.ToFSharpOpt(LineStart),
            CorrespondenceFSharpInterop.ToFSharpOpt(LineEnd),
            CorrespondenceFSharpInterop.ToFSharpOpt(MemberKey),
            Wire,
            CorrespondenceFSharpInterop.ToFSharpOpt(DocLineHint),
            CorrespondenceFSharpInterop.ToFSharpOpt(Excerpt));
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
    public GdlCorrespondence.ExplicitCodeAnchor ToModel() =>
        new(
            DocPath,
            File,
            CorrespondenceFSharpInterop.ToFSharpOpt(LineStart),
            CorrespondenceFSharpInterop.ToFSharpOpt(LineEnd),
            CorrespondenceFSharpInterop.ToFSharpOpt(MemberKey),
            Provenance,
            Kind,
            DefaultKind);
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
    public GdlCorrespondence.CorrespondenceResult ToModel() =>
        new(
            WorkspaceRoot,
            CorrespondenceFSharpInterop.ToFSharpOpt(FileRel),
            CorrespondenceFSharpInterop.ToFSharpOpt(FeatureLine),
            FeatureDocs,
            AdrLine,
            ForwardDocs.Select(d => d.ToModel()).ToArray(),
            ReverseAnchors.Select(a => a.ToModel()).ToArray(),
            ActiveLayers,
            TomlPath);
}

public sealed record ForwardMapResult(
    string? FeatureLine,
    string[] FeatureDocs,
    string AdrLine,
    IReadOnlyList<string> DocPaths,
    ForwardDoc[] ForwardDocs)
{
    public GdlCorrespondence.ForwardMapResult ToModel() =>
        new(
            CorrespondenceFSharpInterop.ToFSharpOpt(FeatureLine),
            FeatureDocs,
            AdrLine,
            ListModule.OfSeq(DocPaths),
            ForwardDocs.Select(d => d.ToModel()).ToArray());
}

internal static class CorrespondenceFSharpInterop
{
    internal static FSharpOption<string> ToFSharpOpt(string? value) =>
        string.IsNullOrEmpty(value) ? FSharpOption<string>.None : FSharpOption<string>.Some(value!);

    internal static FSharpOption<int> ToFSharpOpt(int? value) =>
        value is int v ? FSharpOption<int>.Some(v) : FSharpOption<int>.None;
}
