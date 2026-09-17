#nullable enable
using AIGuiders.Platform.Modeling.Ide.Session;
using AIGuiders.Platform.Navigation;
using AIGuiders.Platform.Navigation.Policy;
using AIGuiders.Platform.Modeling.Paths;

namespace AIGuiders.Platform.Navigation.Code;

public static class NavigationSceneBuilder
{
    [Obsolete("Use BuildRelated(NavSeed, ...) — federation TO-BE plan §8")]
    public static NavigationScene BuildRelated(
        NavigationAnchor anchor,
        IEnumerable<NavigationRelatedItem> candidates,
        NavigationProfile profile) =>
        BuildRelated(NavSeed.FromNavigationAnchor(anchor), candidates, profile);

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
            edges.Add(new NavigationEdge("n0", id, SceneProjection.RelatedToWire, item.Kind));
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
