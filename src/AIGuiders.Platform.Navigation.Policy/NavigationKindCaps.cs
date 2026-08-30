#nullable enable

namespace AIGuiders.Platform.Navigation.Policy;

public static class NavigationKindCaps
{
    public static IReadOnlyDictionary<string, int> DefaultRelated { get; } =
        new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["same_directory"] = 4,
            ["same_namespace"] = 4,
            ["project_peer"] = 3,
        };
}
