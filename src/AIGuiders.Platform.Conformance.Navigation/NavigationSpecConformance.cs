#nullable enable
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
        foreach (var vector in FSharpInterop.ToReadOnlyList(spec.Vectors))
        {
            if (!TryValidateVector(vector, out var error))
                errors.Add(error);
        }

        return errors;
    }

    public static bool TryValidateVector(NavigationSpecVector vector, out string error)
    {
        error = "";
        var profile = NavigationSpecLoader.LoadProfileJson(
            string.IsNullOrWhiteSpace(vector.ProfileJson) ? null : vector.ProfileJson);
        var scene = NavigationCodeExplorer.ExploreRelatedFromWire(vector.WireJson, profile);
        NavigationExpectModel expect;
        try
        {
            expect = NavigationSpecLoader.LoadExpectation(vector.ExpectJson);
        }
        catch (Exception ex)
        {
            error = $"vector \"{vector.Id}\": expect invalid — {ex.Message}";
            return false;
        }

        if (expect.NodeCount > 0 && scene.Nodes.Count != expect.NodeCount)
        {
            error = $"vector \"{vector.Id}\": expected node_count {expect.NodeCount}, got {scene.Nodes.Count}.";
            return false;
        }

        if (expect.Kinds.Length > 0)
        {
            var actualKinds = scene.Nodes.Select(n => n.Kind).ToHashSet(StringComparer.Ordinal);
            foreach (var kind in FSharpInterop.ToReadOnlyList(expect.Kinds))
            {
                if (!actualKinds.Contains(kind))
                {
                    error = $"vector \"{vector.Id}\": expected kind \"{kind}\" missing.";
                    return false;
                }
            }
        }

        if (expect.ExcludedKinds.Length > 0)
        {
            var actualKinds = scene.Nodes.Select(n => n.Kind).ToHashSet(StringComparer.Ordinal);
            foreach (var kind in FSharpInterop.ToReadOnlyList(expect.ExcludedKinds))
            {
                if (actualKinds.Contains(kind))
                {
                    error = $"vector \"{vector.Id}\": excluded kind \"{kind}\" present.";
                    return false;
                }
            }
        }

        if (expect.MaxKindCounts.Length > 0)
        {
            var counts = scene.Nodes
                .GroupBy(n => n.Kind, StringComparer.Ordinal)
                .ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);

            foreach (var pair in FSharpInterop.ToReadOnlyList(expect.MaxKindCounts))
            {
                if (counts.TryGetValue(pair.Item1, out var count) && count > pair.Item2)
                {
                    error = $"vector \"{vector.Id}\": kind \"{pair.Item1}\" count {count} exceeds max {pair.Item2}.";
                    return false;
                }
            }
        }

        return true;
    }
}
