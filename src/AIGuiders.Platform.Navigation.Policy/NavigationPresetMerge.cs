#nullable enable

namespace AIGuiders.Platform.Navigation.Policy;

/// <summary>Merges a named preset from <see cref="NavigationPresets"/> with MCP/host request overrides.</summary>
public static class NavigationPresetMerge
{
    /// <summary>
    /// Non-null <paramref name="requestInclude"/> / <paramref name="requestExclude"/> override the preset side;
    /// when both preset and request specify exclude, lists are unioned (deduped by canonical kind).
    /// </summary>
    public static (IReadOnlyList<string>? Include, IReadOnlyList<string>? Exclude, string? Error) Merge(
        string? presetName,
        IReadOnlyList<string>? requestInclude,
        IReadOnlyList<string>? requestExclude)
    {
        IReadOnlyList<string>? presetInclude = null;
        IReadOnlyList<string>? presetExclude = null;

        if (!string.IsNullOrWhiteSpace(presetName))
        {
            var key = presetName.Trim();
            if (!NavigationPresets.TryGet(key, out var definition))
                return (null, null, $"Неизвестный пресет «{key}»");

            presetInclude = CanonicalizeList(definition.IncludeKinds);
            presetExclude = CanonicalizeList(definition.ExcludeKinds);
        }

        var include = requestInclude ?? presetInclude;

        if (requestExclude is not null && requestExclude.Count > 0)
        {
            if (presetExclude is { Count: > 0 })
            {
                var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var x in presetExclude)
                {
                    var c = NavigationRelatedKinds.TryCanonicalKind(x);
                    if (c is not null)
                        set.Add(c);
                }

                foreach (var x in requestExclude)
                {
                    var c = NavigationRelatedKinds.TryCanonicalKind(x);
                    if (c is not null)
                        set.Add(c);
                }

                return (include, set.OrderBy(x => x, StringComparer.Ordinal).ToList(), null);
            }

            return (include, requestExclude, null);
        }

        return (include, presetExclude ?? Array.Empty<string>(), null);
    }

    static IReadOnlyList<string>? CanonicalizeList(IReadOnlyList<string>? tokens)
    {
        if (tokens is not { Count: > 0 })
            return null;

        var list = new List<string>();
        foreach (var t in tokens)
        {
            var c = NavigationRelatedKinds.TryCanonicalKind(t);
            if (c is not null)
                list.Add(c);
        }

        return list.Count > 0 ? list : null;
    }
}
