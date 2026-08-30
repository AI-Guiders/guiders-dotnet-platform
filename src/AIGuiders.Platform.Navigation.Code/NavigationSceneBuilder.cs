#nullable enable
using AIGuiders.Platform.Navigation;
using AIGuiders.Platform.Navigation.Policy;

namespace AIGuiders.Platform.Navigation.Code;

public static class NavigationSceneBuilder
{
    public static NavigationScene BuildRelated(
        NavigationAnchor anchor,
        IEnumerable<NavigationRelatedItem> candidates,
        NavigationProfile profile)
    {
        var caps = profile.ToCaps();
        var filtered = ApplyFilters(candidates, profile).Take(caps.MaxRelated).ToList();
        var nodes = new List<NavigationNode>
        {
            new(
                "n0",
                anchor.Path,
                "anchor",
                Label: Path.GetFileName(anchor.Path)),
        };
        var edges = new List<NavigationEdge>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { Path.GetFullPath(anchor.Path) };

        var index = 1;
        foreach (var item in filtered)
        {
            string full;
            try
            {
                full = Path.GetFullPath(item.Path);
            }
            catch
            {
                continue;
            }

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
            edges.Add(new NavigationEdge("n0", id, "related_to", item.Kind));
        }

        var kindSummary = nodes
            .Skip(1)
            .GroupBy(n => n.Kind, StringComparer.Ordinal)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key, StringComparer.Ordinal)
            .Select(g => $"{g.Key}×{g.Count()}")
            .ToList();

        var summary = nodes.Count <= 1
            ? NavigationScene.Empty(anchor, NavigationMode.Related, caps).Summary
            : kindSummary.Count == 0
                ? $"Navigation (Related): {nodes.Count - 1} neighbor(s) around {Path.GetFileName(anchor.Path)}."
                : $"Navigation (Related): {nodes.Count - 1} neighbor(s) ({string.Join(", ", kindSummary)}).";

        return new NavigationScene(
            NavigationSchemes.SceneV1,
            NavigationMode.Related,
            anchor,
            nodes,
            edges,
            caps,
            summary);
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
