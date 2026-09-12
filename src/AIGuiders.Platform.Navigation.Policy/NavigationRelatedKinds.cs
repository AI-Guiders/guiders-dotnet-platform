#nullable enable

using ModelingNavPolicy = AIGuiders.Platform.Modeling.Navigation.Policy;

namespace AIGuiders.Platform.Navigation.Policy;

/// <summary>GUIDERS-FSHARP-ADR-0003 §4.8 cutover: canonical related-kind tokens SSOT in Modeling.Navigation.Policy.</summary>
public static class NavigationRelatedKinds
{
    public const string PartialPeer = ModelingNavPolicy.RelatedKinds.PartialPeer;
    public const string ProjectPeer = ModelingNavPolicy.RelatedKinds.ProjectPeer;
    public const string XamlCodeBehindPair = ModelingNavPolicy.RelatedKinds.XamlCodeBehindPair;
    public const string TestCounterpart = ModelingNavPolicy.RelatedKinds.TestCounterpart;
    public const string SameNamespace = ModelingNavPolicy.RelatedKinds.SameNamespace;
    public const string SameDirectory = ModelingNavPolicy.RelatedKinds.SameDirectory;

    public static IReadOnlyList<string> All { get; } =
        FSharpInterop.ToReadOnlyList(ModelingNavPolicy.RelatedKinds.all);

    /// <summary>Canonical kind name or <c>null</c> when the token is unknown.</summary>
    public static string? TryCanonicalKind(string? token) =>
        FSharpInterop.OptString(ModelingNavPolicy.RelatedKinds.tryCanonicalKind(FSharpInterop.OptString(token)));
}
