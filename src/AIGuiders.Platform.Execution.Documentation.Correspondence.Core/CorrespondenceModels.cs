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

    /// <summary>Normalize CRS kind wire to canon token (plan §10 Correspondence.Kind → RelationType).</summary>
    public static string NormalizeWire(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return Documents;

        var parsed = GdlCorrespondence.CorrespondenceRelationKindModule.tryParse(raw.Trim());
        if (FSharpOption<GdlCorrespondence.CorrespondenceRelationKind>.get_IsSome(parsed))
            return GdlCorrespondence.CorrespondenceRelationKindModule.toWire(parsed!.Value);

        return Documents;
    }
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

public sealed record DocToCodeWitness(
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
    public GdlCorrespondence.DocToCodeWitness ToModel()
    {
        var parsedKind = GdlCorrespondence.CorrespondenceRelationKindModule.tryParse(Kind);
        var kind = GdlCorrespondence.CorrespondenceRelationKind.Documents;
        if (FSharpOption<GdlCorrespondence.CorrespondenceRelationKind>.get_IsSome(parsedKind))
            kind = parsedKind!.Value;

        return new(
            DocPath,
            DocTitle,
            Provenance,
            kind,
            File,
            CorrespondenceFSharpInterop.ToFSharpOpt(LineStart),
            CorrespondenceFSharpInterop.ToFSharpOpt(LineEnd),
            CorrespondenceFSharpInterop.ToFSharpOpt(MemberKey),
            Wire,
            CorrespondenceFSharpInterop.ToFSharpOpt(DocLineHint),
            CorrespondenceFSharpInterop.ToFSharpOpt(Excerpt));
    }

    public static DocToCodeWitness FromModel(GdlCorrespondence.DocToCodeWitness model) => new(
        model.DocPath,
        model.DocTitle,
        model.Provenance,
        GdlCorrespondence.CorrespondenceRelationKindModule.toWire(model.Kind),
        model.File,
        model.LineStart is not null && FSharpOption<int>.get_IsSome(model.LineStart) ? model.LineStart.Value : null,
        model.LineEnd is not null && FSharpOption<int>.get_IsSome(model.LineEnd) ? model.LineEnd.Value : null,
        model.MemberKey is not null && FSharpOption<string>.get_IsSome(model.MemberKey) ? model.MemberKey.Value : null,
        model.Wire,
        model.DocLineHint is not null && FSharpOption<int>.get_IsSome(model.DocLineHint) ? model.DocLineHint.Value : null,
        model.Excerpt is not null && FSharpOption<string>.get_IsSome(model.Excerpt) ? model.Excerpt.Value : null);
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
    DocToCodeWitness[] DocToCodeWitnesses,
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
            DocToCodeWitnesses.Select(a => a.ToModel()).ToArray(),
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
