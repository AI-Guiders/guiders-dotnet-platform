#nullable enable
using AIGuiders.Platform.Modeling.Ide.Session;
using AIGuiders.Platform.Navigation;
using AIGuiders.Platform.Navigation.Policy;
using AIGuiders.Platform.Modeling.Paths;

namespace AIGuiders.Platform.Navigation.Code;

public static class NavigationSceneBuilder
{
    public static NavigationScene BuildRelated(
        NavSeed seed,
        IEnumerable<NavigationRelatedItem> candidates,
        NavigationProfile profile)
    {
        var caps = profile.ToCaps();
        var filtered = ApplyFilters(candidates, profile).Take(caps.MaxRelated).ToList();
        var nodes = new List<NavigationNode>
        {
            new(
                "n0",
                seed.Path,
                "anchor",
                Label: Path.GetFileName(seed.Path)),
        };
        var edges = new List<NavigationEdge>();
        var anchorKey = PathBoundary.TryCanonicalPhysical(seed.Path) ?? seed.Path;
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { anchorKey };

        var index = 1;
        foreach (var item in filtered)
        {
            var full = PathBoundary.TryCanonicalPhysical(item.Path);
            if (full is null)
                continue;

            if (!seen.Add(full))
                continue;

            var id = $"n{index++}";
            nodes.Add(new NavigationNode(
                id,
                full,
                item.Kind,
                item.Rationale,
                item.RelativePath,
                Path.GetFileName(full)));
            edges.Add(SceneProjectionBridge.ProjectRelatedNeighbor("n0", id, item.Kind));
        }

        var kindSummary = nodes
            .Skip(1)
            .GroupBy(n => n.Kind, StringComparer.Ordinal)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key, StringComparer.Ordinal)
            .Select(g => $"{g.Key}×{g.Count()}")
            .ToList();

        var summary = nodes.Count <= 1
            ? NavigationScene.Empty(seed, NavigationMode.Related, caps).Summary
            : kindSummary.Count == 0
                ? $"Navigation (Related): {nodes.Count - 1} neighbor(s) around {Path.GetFileName(seed.Path)}."
                : $"Navigation (Related): {nodes.Count - 1} neighbor(s) ({string.Join(", ", kindSummary)}).";

        return new NavigationScene(
            NavigationSchemes.SceneV1,
            NavigationMode.Related,
            seed,
            nodes,
            edges,
            caps,
            summary);
    }

    /// <summary>
    /// Build a subgraph scene from pre-mapped node ids and persisted G relations (projection only).
    /// </summary>
    public static NavigationScene BuildSubgraph(
        NavSeed seed,
        IReadOnlyList<NavigationNode> nodes,
        IEnumerable<(string FromId, string ToId, Relation Relation)> relationProjections,
        NavigationProfile profile,
        string? summary = null)
    {
        var caps = profile.ToCaps();
        var edges = relationProjections
            .Select(p => SceneProjectionBridge.ProjectRelation(p.FromId, p.ToId, p.Relation))
            .Take(caps.MaxEdges)
            .ToList();

        var fileName = Path.GetFileName(seed.Path);
        var resolvedSummary = summary
            ?? $"Navigation (Subgraph): {nodes.Count} node(s), {edges.Count} relation edge(s) around {fileName}.";

        return new NavigationScene(
            NavigationSchemes.SceneV1,
            NavigationMode.Subgraph,
            seed,
            nodes,
            edges,
            caps,
            resolvedSummary);
    }

    static IEnumerable<NavigationRelatedItem> ApplyFilters(
        IEnumerable<NavigationRelatedItem> candidates,
        NavigationProfile profile)
    {
        var kindCounts = new Dictionary<string, int>(StringComparer.Ordinal);
        var caps = NavigationKindCaps.DefaultRelated;
        var (include, exclude, _) = NavigationPresetMerge.Merge(
            profile.Preset,
            profile.IncludeKinds,
            profile.ExcludeKinds);
        var kindFilter = NavigationKindFilter.Create(include, exclude);

        foreach (var item in candidates)
        {
            if (!kindFilter.Allows(item.Kind))
                continue;

            if (caps.TryGetValue(item.Kind, out var cap))
            {
                kindCounts.TryGetValue(item.Kind, out var used);
                if (used >= cap)
                    continue;
                kindCounts[item.Kind] = used + 1;
            }

            yield return item;
        }
    }
}
