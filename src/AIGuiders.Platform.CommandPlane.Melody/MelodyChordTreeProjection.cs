#nullable enable

namespace AIGuiders.Platform.CommandPlane.Melody;

public enum MelodyGraphNodeKind
{
    SubRoot,
    Command,
}

public enum MelodyChordTreeViewMode
{
    /// <summary>Breadcrumb + top few next hops (default daily).</summary>
    Minimal,

    /// <summary>Current node neighborhood — children and near siblings.</summary>
    Neighborhood,

    /// <summary>Exploration map — larger slice of the trie (planet caps cost).</summary>
    Full,
}

public sealed record MelodyGraphEdge(
    string Label,
    string? Hint,
    MelodyGraphNodeKind Kind,
    string TargetNodeId,
    string? PreviewWire = null);

/// <summary>Headless projection for native Visual Chord Tree ports (GUIDERS-ADR-0024).</summary>
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
    public const int DefaultMinimalLimit = 5;
    public const int DefaultNeighborhoodLimit = 24;

    public static MelodyChordTreeProjection Project(
        MelodyCaptureStack stack,
        IMelodyGraphCatalog catalog,
        MelodyChordTreeViewMode viewMode = MelodyChordTreeViewMode.Neighborhood,
        int? optionLimit = null)
    {
        ArgumentNullException.ThrowIfNull(stack);
        ArgumentNullException.ThrowIfNull(catalog);

        var current = stack.Current
            ?? new MelodyCaptureFrame(catalog.RootNodeId, "", MelodyLineProfile.PureByNote);

        var breadcrumb = stack.Frames.Count == 0
            ? [catalog.RootNodeId]
            : stack.Frames.Select(static frame => frame.NodeId).ToArray();

        if (!catalog.TryGetEdges(current.NodeId, out var edges))
            edges = [];

        var filtered = FilterByConsumedPrefix(edges, current.ConsumedPrefix);
        var limit = optionLimit ?? LimitFor(viewMode);
        var next = filtered.Take(limit).ToArray();

        return new MelodyChordTreeProjection(
            viewMode,
            breadcrumb,
            current.ConsumedPrefix,
            next,
            viewMode == MelodyChordTreeViewMode.Full ? filtered : null);
    }

    public static IReadOnlyList<MelodyGraphEdge> FilterByConsumedPrefix(
        IReadOnlyList<MelodyGraphEdge> edges,
        string consumedPrefix)
    {
        if (string.IsNullOrEmpty(consumedPrefix))
            return edges;

        return edges
            .Where(edge => edge.Label.StartsWith(consumedPrefix, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    static int LimitFor(MelodyChordTreeViewMode viewMode) => viewMode switch
    {
        MelodyChordTreeViewMode.Minimal => DefaultMinimalLimit,
        MelodyChordTreeViewMode.Neighborhood => DefaultNeighborhoodLimit,
        MelodyChordTreeViewMode.Full => int.MaxValue,
        _ => DefaultNeighborhoodLimit,
    };
}

/// <summary>Empty catalog for tests and planets without trie metadata yet.</summary>
public sealed class MelodyGraphCatalogEmpty : IMelodyGraphCatalog
{
    public static MelodyGraphCatalogEmpty Instance { get; } = new();

    public string RootNodeId => "root";

    public bool TryGetEdges(string nodeId, out IReadOnlyList<MelodyGraphEdge> edges)
    {
        edges = [];
        return false;
    }
}
