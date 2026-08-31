#nullable enable

using AIGuiders.Platform.CommandPlane;
using AIGuiders.Platform.IntermediateRepresentation.Invocation;

namespace AIGuiders.Platform.CommandPlane.Melody;

public enum MelodyGraphNodeKind
{
    SubRoot,
    Command,
}

public enum MelodyChordTreeViewMode
{
    Minimal,
    Neighborhood,
    Full,
}

public sealed record MelodyGraphEdge(
    string Label,
    string? Hint,
    MelodyGraphNodeKind Kind,
    string TargetNodeId,
    string? PreviewWire = null);

public sealed record MelodyChordTreeProjection(
    MelodyChordTreeViewMode ViewMode,
    IReadOnlyList<string> Breadcrumb,
    string ConsumedPrefix,
    IReadOnlyList<MelodyGraphEdge> NextOptions,
    IReadOnlyList<MelodyGraphEdge>? ExtendedGraph = null);

public interface IMelodyGraphCatalog
{
    string RootNodeId { get; }

    bool TryGetEdges(string nodeId, out IReadOnlyList<MelodyGraphEdge> edges);
}

public static class MelodyChordTreeProjector
{
    public const int DefaultMinimalLimit = VisualCommandTreeProjector.DefaultMinimalLimit;
    public const int DefaultNeighborhoodLimit = VisualCommandTreeProjector.DefaultNeighborhoodLimit;

    public static MelodyChordTreeProjection Project(
        MelodyCaptureStack stack,
        IMelodyGraphCatalog catalog,
        MelodyChordTreeViewMode viewMode = MelodyChordTreeViewMode.Neighborhood,
        int? optionLimit = null)
    {
        ArgumentNullException.ThrowIfNull(stack);
        ArgumentNullException.ThrowIfNull(catalog);

        var frames = stack.Frames
            .Select(frame => new VisualCommandTreeFrame(frame.NodeId, frame.ConsumedPrefix, frame.NodeId))
            .ToArray();

        var projection = VisualCommandTreeProjector.ProjectCapture(
            frames,
            new MelodyCatalogAdapter(catalog),
            InvocationEngageKind.Melody,
            argMechanic: null,
            ToShared(viewMode),
            optionLimit);

        return FromShared(projection);
    }

    public static IReadOnlyList<MelodyGraphEdge> FilterByConsumedPrefix(
        IReadOnlyList<MelodyGraphEdge> edges,
        string consumedPrefix) =>
        VisualCommandTreeProjector
            .FilterByConsumedPrefix(edges.Select(ToShared).ToArray(), consumedPrefix)
            .Select(FromShared)
            .ToArray();

    static MelodyChordTreeProjection FromShared(VisualCommandTreeProjection projection) =>
        new(
            FromShared(projection.ViewMode),
            projection.BreadcrumbSegments,
            projection.ConsumedPrefix,
            projection.NextOptions.Select(FromShared).ToArray(),
            projection.ExtendedGraph?.Select(FromShared).ToArray());

    static VisualCommandTreeViewMode ToShared(MelodyChordTreeViewMode viewMode) => viewMode switch
    {
        MelodyChordTreeViewMode.Minimal => VisualCommandTreeViewMode.Minimal,
        MelodyChordTreeViewMode.Full => VisualCommandTreeViewMode.Full,
        _ => VisualCommandTreeViewMode.Neighborhood,
    };

    static MelodyChordTreeViewMode FromShared(VisualCommandTreeViewMode viewMode) => viewMode switch
    {
        VisualCommandTreeViewMode.Minimal => MelodyChordTreeViewMode.Minimal,
        VisualCommandTreeViewMode.Full => MelodyChordTreeViewMode.Full,
        _ => MelodyChordTreeViewMode.Neighborhood,
    };

    static VisualCommandTreeEdge ToShared(MelodyGraphEdge edge) =>
        new(
            edge.Label,
            edge.Hint,
            edge.Kind == MelodyGraphNodeKind.SubRoot
                ? VisualCommandTreeNodeKind.SubRoot
                : VisualCommandTreeNodeKind.Command,
            edge.TargetNodeId,
            PreviewWire: edge.PreviewWire);

    static MelodyGraphEdge FromShared(VisualCommandTreeEdge edge) =>
        new(
            edge.Label,
            edge.Hint,
            edge.Kind == VisualCommandTreeNodeKind.SubRoot
                ? MelodyGraphNodeKind.SubRoot
                : MelodyGraphNodeKind.Command,
            edge.TargetNodeId,
            edge.PreviewWire);

    sealed class MelodyCatalogAdapter(IMelodyGraphCatalog catalog) : IVisualCommandTreeCatalog
    {
        public string RootNodeId => catalog.RootNodeId;

        public bool TryGetEdges(string nodeId, out IReadOnlyList<VisualCommandTreeEdge> edges)
        {
            if (!catalog.TryGetEdges(nodeId, out var melodyEdges))
            {
                edges = [];
                return false;
            }

            edges = melodyEdges.Select(ToShared).ToArray();
            return true;
        }
    }
}

public sealed class MelodyGraphCatalogEmpty : IMelodyGraphCatalog
{
    public static MelodyGraphCatalogEmpty Instance { get; } = new();

    public string RootNodeId => VisualCommandTreeCatalogEmpty.Instance.RootNodeId;

    public bool TryGetEdges(string nodeId, out IReadOnlyList<MelodyGraphEdge> edges)
    {
        edges = [];
        return VisualCommandTreeCatalogEmpty.Instance.TryGetEdges(nodeId, out _);
    }
}
