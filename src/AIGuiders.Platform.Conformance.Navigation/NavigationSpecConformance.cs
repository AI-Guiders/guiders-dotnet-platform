#nullable enable
using System.Text.Json;
using AIGuiders.Platform.Navigation.Code;

namespace AIGuiders.Platform.Conformance.Navigation;

public static class NavigationSpecConformance
{
    public static IReadOnlyList<string> ValidateDocument(NavigationSpecDocument spec)
    {
        if (!string.Equals(spec.Kind, "navigation", StringComparison.Ordinal))
            return [$"Expected kind \"navigation\", got \"{spec.Kind}\"."];

        if (!string.Equals(spec.Surface, "code.explore-scene", StringComparison.Ordinal))
            return [$"Unsupported surface \"{spec.Surface}\" (v0.29: code.explore-scene only)."];

        var errors = new List<string>();
        foreach (var vector in spec.Vectors)
        {
            if (!TryValidateVector(vector, out var error))
                errors.Add(error);
        }

        return errors;
    }

    public static bool TryValidateVector(NavigationSpecVector vector, out string error)
    {
        error = "";
        var profile = NavigationSpecLoader.LoadProfile(vector.Profile);
        var wireJson = vector.Wire.GetRawText();
        var scene = NavigationCodeExplorer.ExploreRelatedFromWire(wireJson, profile);
        var expect = vector.Expect.Deserialize<NavigationExpectWire>(NavigationSpecLoader.JsonOptions);
        if (expect is null)
        {
            error = $"vector \"{vector.Id}\": expect missing.";
            return false;
        }

        if (expect.NodeCount is { } nodeCount && scene.Nodes.Count != nodeCount)
        {
            error = $"vector \"{vector.Id}\": expected node_count {nodeCount}, got {scene.Nodes.Count}.";
            return false;
        }

        if (expect.Kinds is { Count: > 0 })
        {
            var actualKinds = scene.Nodes.Select(n => n.Kind).ToHashSet(StringComparer.Ordinal);
            foreach (var kind in expect.Kinds)
            {
                if (!actualKinds.Contains(kind))
                {
                    error = $"vector \"{vector.Id}\": expected kind \"{kind}\" missing.";
                    return false;
                }
            }
        }

        if (expect.ExcludedKinds is { Count: > 0 })
        {
            var actualKinds = scene.Nodes.Select(n => n.Kind).ToHashSet(StringComparer.Ordinal);
            foreach (var kind in expect.ExcludedKinds)
            {
                if (actualKinds.Contains(kind))
                {
                    error = $"vector \"{vector.Id}\": excluded kind \"{kind}\" present.";
                    return false;
                }
            }
        }

        if (expect.MaxKindCount is { Count: > 0 })
        {
            var counts = scene.Nodes
                .GroupBy(n => n.Kind, StringComparer.Ordinal)
                .ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);

            foreach (var (kind, max) in expect.MaxKindCount)
            {
                if (counts.TryGetValue(kind, out var count) && count > max)
                {
                    error = $"vector \"{vector.Id}\": kind \"{kind}\" count {count} exceeds max {max}.";
                    return false;
                }
            }
        }

        return true;
    }
}
