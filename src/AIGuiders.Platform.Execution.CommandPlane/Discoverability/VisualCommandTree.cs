#nullable enable

using AIGuiders.Platform.IntermediateRepresentation.Invocation;

namespace AIGuiders.Platform.Execution.CommandPlane;

/// <summary>Next-hop kind in a projected command tree.</summary>
public enum VisualCommandTreeNodeKind
{
    SubRoot,
    Command,
    Segment,
    Picker,
    ConstructorEntry,
    ConstructorStep,
    Instant,
}

/// <summary>Projection density — caps layout cost on native surfaces.</summary>
public enum VisualCommandTreeViewMode
{
    Minimal,
    Neighborhood,
    Full,
}

/// <summary>One frame in a recursive capture stack (melody trie node, slash path segment, constructor slot).</summary>
public sealed record VisualCommandTreeFrame(
    string NodeId,
    string ConsumedPrefix,
    string? Label = null);

/// <summary>One selectable next hop in the visual command tree.</summary>
public sealed record VisualCommandTreeEdge(
    string Label,
    string? Hint,
    VisualCommandTreeNodeKind Kind,
    string TargetNodeId,
    string? PickValue = null,
    string? PreviewWire = null);

/// <summary>
/// Headless "where am I / what is next" projection for native surfaces (CCL, chord HUD, palette).
/// </summary>
public sealed record VisualCommandTreeProjection(
    VisualCommandTreeViewMode ViewMode,
    InvocationEngageKind EngageKind,
    ArgMechanic? ArgMechanic,
    IReadOnlyList<string> BreadcrumbSegments,
    string BreadcrumbDisplay,
    string ConsumedPrefix,
    string Placeholder,
    string NextStepHint,
    string? InputMode,
    IReadOnlyList<VisualCommandTreeEdge> NextOptions,
    IReadOnlyList<VisualCommandTreeEdge>? ExtendedGraph = null);

public interface IVisualCommandTreeCatalog
{
    string RootNodeId { get; }

    bool TryGetEdges(string nodeId, out IReadOnlyList<VisualCommandTreeEdge> edges);
}

public static class VisualCommandTreeProjector
{
    public const int DefaultMinimalLimit = 5;
    public const int DefaultNeighborhoodLimit = 24;

    public static VisualCommandTreeProjection ProjectCapture(
        IReadOnlyList<VisualCommandTreeFrame> frames,
        IVisualCommandTreeCatalog catalog,
        InvocationEngageKind engageKind = InvocationEngageKind.Melody,
        ArgMechanic? argMechanic = null,
        VisualCommandTreeViewMode viewMode = VisualCommandTreeViewMode.Neighborhood,
        int? optionLimit = null)
    {
        ArgumentNullException.ThrowIfNull(frames);
        ArgumentNullException.ThrowIfNull(catalog);

        var current = frames.Count == 0
            ? new VisualCommandTreeFrame(catalog.RootNodeId, "")
            : frames[^1];

        var breadcrumbSegments = frames.Count == 0
            ? [catalog.RootNodeId]
            : frames.Select(static frame => frame.Label ?? frame.NodeId).ToArray();

        if (!catalog.TryGetEdges(current.NodeId, out var edges))
        {
            edges = [];
        }

        var filtered = FilterByConsumedPrefix(edges, current.ConsumedPrefix);
        var limit = optionLimit ?? LimitFor(viewMode);
        var next = filtered.Take(limit).ToArray();

        return new VisualCommandTreeProjection(
            viewMode,
            engageKind,
            argMechanic,
            breadcrumbSegments,
            BuildBreadcrumbDisplay(breadcrumbSegments),
            current.ConsumedPrefix,
            Placeholder: "Continue input",
            NextStepHint: next.Length > 0 ? next[0].Hint ?? next[0].Label : "Continue",
            InputMode: argMechanic?.ToString() ?? engageKind.ToString(),
            next,
            viewMode == VisualCommandTreeViewMode.Full ? filtered : null);
    }

    public static IReadOnlyList<VisualCommandTreeEdge> FilterByConsumedPrefix(
        IReadOnlyList<VisualCommandTreeEdge> edges,
        string consumedPrefix)
    {
        if (string.IsNullOrEmpty(consumedPrefix))
        {
            return edges;
        }

        return edges
            .Where(edge => edge.Label.StartsWith(consumedPrefix, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    public static string BuildBreadcrumbDisplay(IReadOnlyList<string> segments) =>
        segments.Count == 0 ? "/" : "/" + string.Join(" › ", segments);

    static int LimitFor(VisualCommandTreeViewMode viewMode) => viewMode switch
    {
        VisualCommandTreeViewMode.Minimal => DefaultMinimalLimit,
        VisualCommandTreeViewMode.Neighborhood => DefaultNeighborhoodLimit,
        VisualCommandTreeViewMode.Full => int.MaxValue,
        _ => DefaultNeighborhoodLimit,
    };
}

public sealed class VisualCommandTreeCatalogEmpty : IVisualCommandTreeCatalog
{
    public static VisualCommandTreeCatalogEmpty Instance { get; } = new();

    public string RootNodeId => "root";

    public bool TryGetEdges(string nodeId, out IReadOnlyList<VisualCommandTreeEdge> edges)
    {
        edges = [];
        return false;
    }
}
