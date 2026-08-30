#nullable enable
using System.Text.Json;

namespace AIGuiders.Platform.Navigation.Policy;

public sealed record NavigationPresetDefinition(
    IReadOnlyList<string>? IncludeKinds,
    IReadOnlyList<string>? ExcludeKinds);

public static class NavigationPresets
{
    static readonly IReadOnlyDictionary<string, NavigationPresetDefinition> Catalog =
        new Dictionary<string, NavigationPresetDefinition>(StringComparer.Ordinal)
        {
            ["peers_only"] = new([NavigationRelatedKinds.PartialPeer, NavigationRelatedKinds.ProjectPeer], null),
            ["no_namespace_noise"] = new(null, [NavigationRelatedKinds.SameNamespace, NavigationRelatedKinds.SameDirectory]),
            ["tests_and_peers"] = new(
                [NavigationRelatedKinds.PartialPeer, NavigationRelatedKinds.ProjectPeer, NavigationRelatedKinds.TestCounterpart],
                null),
            ["structure_only"] = new(
                [
                    NavigationRelatedKinds.PartialPeer,
                    NavigationRelatedKinds.ProjectPeer,
                    NavigationRelatedKinds.XamlCodeBehindPair,
                    NavigationRelatedKinds.SameDirectory,
                ],
                null),
            ["explore_default"] = new(null, [NavigationRelatedKinds.ProjectPeer]),
        };

    public static bool TryGet(string? presetId, out NavigationPresetDefinition definition)
    {
        if (string.IsNullOrWhiteSpace(presetId))
        {
            definition = new NavigationPresetDefinition(null, null);
            return true;
        }

        return Catalog.TryGetValue(presetId.Trim(), out definition!);
    }

    public static bool AllowsKind(string? presetId, string kind)
    {
        if (!TryGet(presetId, out var definition))
            return false;

        if (definition.IncludeKinds is { Count: > 0 } include
            && !include.Contains(kind, StringComparer.Ordinal))
            return false;

        if (definition.ExcludeKinds is { Count: > 0 } exclude
            && exclude.Contains(kind, StringComparer.Ordinal))
            return false;

        return true;
    }

    public static string CatalogJson() =>
        JsonSerializer.Serialize(
            Catalog.ToDictionary(
                pair => pair.Key,
                pair => new
                {
                    include_kinds = pair.Value.IncludeKinds,
                    exclude_kinds = pair.Value.ExcludeKinds,
                }),
            new JsonSerializerOptions { WriteIndented = true });
}
