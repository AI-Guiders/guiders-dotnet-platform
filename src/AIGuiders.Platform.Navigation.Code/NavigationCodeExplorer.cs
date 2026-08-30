#nullable enable
using AIGuiders.Platform.Navigation;
using AIGuiders.Platform.Navigation.Policy;

namespace AIGuiders.Platform.Navigation.Code;

public static class NavigationCodeExplorer
{
    public static NavigationScene ExploreRelatedFromWire(string roslynWireJson, NavigationProfile profile)
    {
        if (!NavigationWireParser.TryParseRelatedWire(roslynWireJson, out var anchor, out var items, out _))
            return NavigationScene.Empty(
                new NavigationAnchor(""),
                NavigationMode.Related,
                profile.ToCaps());

        return NavigationSceneBuilder.BuildRelated(anchor, items, profile);
    }

    public static NavigationScene ExploreRelatedInMemory(
        NavigationAnchor anchor,
        IReadOnlyList<string> solutionFiles,
        NavigationProfile profile)
    {
        var items = InMemoryRelatedProvider.Collect(anchor.Path, solutionFiles);
        return NavigationSceneBuilder.BuildRelated(anchor, items, profile);
    }
}

internal static class InMemoryRelatedProvider
{
    public static IReadOnlyList<NavigationRelatedItem> Collect(string anchorPath, IReadOnlyList<string> solutionFiles)
    {
        var anchorFull = Path.GetFullPath(anchorPath);
        var anchorDir = Path.GetDirectoryName(anchorFull);
        var anchorStem = Path.GetFileNameWithoutExtension(anchorFull);
        var list = new List<NavigationRelatedItem>();

        if (!string.IsNullOrEmpty(anchorDir))
        {
            foreach (var file in solutionFiles
                         .Where(f => string.Equals(Path.GetDirectoryName(Path.GetFullPath(f)), anchorDir, StringComparison.OrdinalIgnoreCase))
                         .Where(f => !string.Equals(Path.GetFullPath(f), anchorFull, StringComparison.OrdinalIgnoreCase))
                         .OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
            {
                var stem = Path.GetFileNameWithoutExtension(file);
                if (stem.Equals(anchorStem + "Tests", StringComparison.OrdinalIgnoreCase)
                    || stem.Equals(anchorStem + "Test", StringComparison.OrdinalIgnoreCase))
                {
                    list.Add(new NavigationRelatedItem(
                        file,
                        "test_counterpart",
                        "Test counterpart",
                        RelativePath: Path.GetFileName(file)));
                    continue;
                }

                list.Add(new NavigationRelatedItem(
                    file,
                    "same_directory",
                    "Same directory peer",
                    RelativePath: Path.GetFileName(file)));
            }
        }

        return list;
    }
}
