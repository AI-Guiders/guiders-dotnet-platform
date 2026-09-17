#nullable enable

using ModelingNavigation = AIGuiders.Platform.Modeling.Navigation;

namespace AIGuiders.Platform.Navigation;

/// <summary>GUIDERS-FSHARP-ADR-0003 §4.8 cutover: scene IR SSOT via <see cref="NavSeed.ToModel"/> (Modeling.Navigation).</summary>
public static class NavigationSchemes
{
    public const string SceneV1 = ModelingNavigation.Schemes.SceneV1;
}

public enum NavigationMode
{
    Related,
    Subgraph,
}

public enum NavigationDomain
{
    Code,
    Docs,
    Workspace,
}

/// <summary>Federation navigation seed — primary platform type per plan §8.</summary>
public sealed record NavSeed(
    string Path,
    int? Line = null,
    int? Column = null,
    string? Command = null,
    string? Go = null,
    string? SolutionPath = null)
{
    public ModelingNavigation.Anchor ToModel() =>
        new(
            Path,
            FSharpInterop.OptInt(Line),
            FSharpInterop.OptInt(Column),
            FSharpInterop.OptString(SolutionPath));

    public static NavSeed FromModel(ModelingNavigation.Anchor model) => new(
        model.Path,
        FSharpInterop.OptInt(model.Line),
        FSharpInterop.OptInt(model.Column),
        FSharpInterop.OptString(model.SolutionPath));
}

public sealed record NavigationNode(
    string Id,
    string Path,
    string Kind,
    string? Rationale = null,
    string? RelativePath = null,
    string? Label = null)
{
    public ModelingNavigation.Node ToModel() =>
        new(
            Id,
            Path,
            Kind,
            FSharpInterop.OptString(Rationale),
            FSharpInterop.OptString(RelativePath),
            FSharpInterop.OptString(Label));

    public static NavigationNode FromModel(ModelingNavigation.Node model) => new(
        model.Id,
        model.Path,
        model.Kind,
        FSharpInterop.OptString(model.Rationale),
        FSharpInterop.OptString(model.RelativePath),
        FSharpInterop.OptString(model.Label));
}

public sealed record NavigationEdge(
    string FromId,
    string ToId,
    string Kind,
    string? RelatedKind = null)
{
    public ModelingNavigation.Edge ToModel() =>
        new(
            FromId,
            ToId,
            Kind,
            FSharpInterop.OptString(RelatedKind));

    public static NavigationEdge FromModel(ModelingNavigation.Edge model) => new(
        model.FromId,
        model.ToId,
        model.Kind,
        FSharpInterop.OptString(model.RelatedKind));
}

public sealed record NavigationSceneCaps(
    int MaxRelated,
    int MaxNodes,
    int MaxEdges,
    string? Preset,
    IReadOnlyDictionary<string, int>? KindCaps = null)
{
    public ModelingNavigation.SceneCaps ToModel() =>
        new(
            MaxRelated,
            MaxNodes,
            MaxEdges,
            FSharpInterop.OptString(Preset),
            FSharpInterop.ToFSharpMap(KindCaps));

    public static NavigationSceneCaps FromModel(ModelingNavigation.SceneCaps model) => new(
        model.MaxRelated,
        model.MaxNodes,
        model.MaxEdges,
        FSharpInterop.OptString(model.Preset),
        FSharpInterop.ToReadOnlyDict(model.KindCaps));
}

public sealed record NavigationScene(
    string Schema,
    NavigationMode Mode,
    NavSeed Seed,
    IReadOnlyList<NavigationNode> Nodes,
    IReadOnlyList<NavigationEdge> Edges,
    NavigationSceneCaps Caps,
    string Summary)
{
    public static NavigationScene Empty(NavSeed seed, NavigationMode mode, NavigationSceneCaps caps) =>
        FromModel(ModelingNavigation.SceneModule.empty(seed.ToModel(), ToMode(mode), caps.ToModel()));

    public ModelingNavigation.Scene ToModel() =>
        new(
            Schema,
            ToMode(Mode),
            Seed.ToModel(),
            FSharpInterop.ToFSharpList(Nodes.Select(n => n.ToModel()).ToList()),
            FSharpInterop.ToFSharpList(Edges.Select(e => e.ToModel()).ToList()),
            Caps.ToModel(),
            Summary);

    public static NavigationScene FromModel(ModelingNavigation.Scene model) => new(
        model.Schema,
        FromMode(model.Mode),
        NavSeed.FromModel(model.Anchor),
        FSharpInterop.ToReadOnlyList(model.Nodes).Select(NavigationNode.FromModel).ToList(),
        FSharpInterop.ToReadOnlyList(model.Edges).Select(NavigationEdge.FromModel).ToList(),
        NavigationSceneCaps.FromModel(model.Caps),
        model.Summary);

    static ModelingNavigation.Mode ToMode(NavigationMode mode) => mode switch
    {
        NavigationMode.Related => ModelingNavigation.Mode.Related,
        NavigationMode.Subgraph => ModelingNavigation.Mode.Subgraph,
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null),
    };

    static NavigationMode FromMode(ModelingNavigation.Mode mode) =>
        mode == ModelingNavigation.Mode.Subgraph
            ? NavigationMode.Subgraph
            : NavigationMode.Related;
}
