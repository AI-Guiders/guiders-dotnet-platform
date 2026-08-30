#nullable enable

namespace AIGuiders.Platform.Navigation;

public static class NavigationSchemes
{
    public const string SceneV1 = "navigation_scene/v1";
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

public sealed record NavigationAnchor(
    string Path,
    int? Line = null,
    int? Column = null,
    string? SolutionPath = null);

public sealed record NavigationNode(
    string Id,
    string Path,
    string Kind,
    string? Rationale = null,
    string? RelativePath = null,
    string? Label = null);

public sealed record NavigationEdge(
    string FromId,
    string ToId,
    string Kind,
    string? RelatedKind = null);

public sealed record NavigationSceneCaps(
    int MaxRelated,
    int MaxNodes,
    int MaxEdges,
    string? Preset,
    IReadOnlyDictionary<string, int>? KindCaps = null);

public sealed record NavigationScene(
    string Schema,
    NavigationMode Mode,
    NavigationAnchor Anchor,
    IReadOnlyList<NavigationNode> Nodes,
    IReadOnlyList<NavigationEdge> Edges,
    NavigationSceneCaps Caps,
    string Summary)
{
    public static NavigationScene Empty(NavigationAnchor anchor, NavigationMode mode, NavigationSceneCaps caps) =>
        new(
            NavigationSchemes.SceneV1,
            mode,
            anchor,
            [],
            [],
            caps,
            $"Navigation ({mode}): no neighbors for {Path.GetFileName(anchor.Path)}.");
}
