#nullable enable

namespace AIGuiders.Platform.Navigation.Policy;

/// <summary>
/// Kind filter: non-empty <c>includeKinds</c> is a whitelist; <c>excludeKinds</c> subtracts.
/// Unknown tokens in either list are ignored.
/// </summary>
public readonly struct NavigationKindFilter
{
    readonly HashSet<string>? _include;
    readonly HashSet<string> _exclude;

    NavigationKindFilter(HashSet<string>? include, HashSet<string> exclude)
    {
        _include = include;
        _exclude = exclude;
    }

    /// <summary><c>null</c> when no whitelist (all kinds except excluded).</summary>
    public IReadOnlyList<string>? EffectiveIncludeKinds =>
        _include is null ? null : _include.OrderBy(x => x, StringComparer.Ordinal).ToList();

    public IReadOnlyList<string> EffectiveExcludeKinds =>
        _exclude.Count == 0 ? Array.Empty<string>() : _exclude.OrderBy(x => x, StringComparer.Ordinal).ToList();

    public static NavigationKindFilter Create(IReadOnlyList<string>? includeKinds, IReadOnlyList<string>? excludeKinds)
    {
        var exclude = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (excludeKinds is not null)
        {
            foreach (var t in excludeKinds)
            {
                var c = NavigationRelatedKinds.TryCanonicalKind(t);
                if (c is not null)
                    exclude.Add(c);
            }
        }

        HashSet<string>? include = null;
        if (includeKinds is not null && includeKinds.Count > 0)
        {
            include = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var t in includeKinds)
            {
                var c = NavigationRelatedKinds.TryCanonicalKind(t);
                if (c is not null)
                    include.Add(c);
            }

            if (include.Count == 0)
                include = null;
        }

        return new NavigationKindFilter(include, exclude);
    }

    public bool Allows(string kind)
    {
        if (string.IsNullOrEmpty(kind))
            return false;
        if (_include is not null && !_include.Contains(kind))
            return false;
        if (_exclude.Contains(kind))
            return false;
        return true;
    }
}
