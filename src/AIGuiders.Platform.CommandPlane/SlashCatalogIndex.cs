#nullable enable

namespace AIGuiders.Platform.CommandPlane;

/// <summary>Longest-prefix slash catalog (bundled + overlay merge). ADR-0153.</summary>
public sealed class SlashCatalogIndex
{
    readonly Dictionary<string, SlashRouteEntry> _byPath;
    readonly string[] _pathsLongestFirst;

    SlashCatalogIndex(Dictionary<string, SlashRouteEntry> byPath, string[] pathsLongestFirst)
    {
        _byPath = byPath;
        _pathsLongestFirst = pathsLongestFirst;
    }

    public static SlashCatalogIndex Empty { get; } = new(new(StringComparer.OrdinalIgnoreCase), []);

    public static SlashCatalogIndex FromDescriptors(IEnumerable<SlashCommandDescriptor> descriptors)
    {
        var byPath = new Dictionary<string, SlashRouteEntry>(StringComparer.OrdinalIgnoreCase);
        foreach (var d in descriptors)
        {
            foreach (var path in d.AllPaths())
            {
                var normalized = NormalizePath(path);
                if (normalized.Length == 0 || byPath.ContainsKey(normalized))
                    continue;
                byPath[normalized] = SlashRouteEntry.FromDescriptor(d, normalized);
            }
        }

        var longest = byPath.Keys.OrderByDescending(p => p.Length).ThenBy(p => p, StringComparer.OrdinalIgnoreCase).ToArray();
        return new SlashCatalogIndex(byPath, longest);
    }

    public SlashCatalogIndex Merge(SlashCatalogIndex overlay)
    {
        var merged = new Dictionary<string, SlashRouteEntry>(_byPath, StringComparer.OrdinalIgnoreCase);
        foreach (var (k, v) in overlay._byPath)
            merged.TryAdd(k, v);
        var longest = merged.Keys.OrderByDescending(p => p.Length).ThenBy(p => p, StringComparer.OrdinalIgnoreCase).ToArray();
        return new SlashCatalogIndex(merged, longest);
    }

    public bool TryGet(string slashPath, out SlashRouteEntry entry) =>
        _byPath.TryGetValue(NormalizePath(slashPath), out entry);

    public bool TryResolveLongestPrefix(
        IReadOnlyList<string> tokens,
        bool endsWithSpace,
        out string canonicalPath,
        out string argTail,
        out bool isExactPath,
        out bool endsWithSpaceAfterPath,
        out SlashRouteEntry entry)
    {
        canonicalPath = "";
        argTail = "";
        isExactPath = false;
        endsWithSpaceAfterPath = false;
        entry = default;
        if (tokens.Count == 0)
            return false;

        for (var take = tokens.Count; take >= 1; take--)
        {
            var candidate = string.Join(' ', tokens.Take(take));
            if (!_byPath.TryGetValue(candidate, out var route))
                continue;

            entry = route;
            canonicalPath = candidate;
            isExactPath = take == tokens.Count && endsWithSpace;
            endsWithSpaceAfterPath = take < tokens.Count ? false : endsWithSpace;
            if (take < tokens.Count)
                argTail = string.Join(' ', tokens.Skip(take));
            return true;
        }

        return false;
    }

    static string NormalizePath(string path)
    {
        var p = path.Trim();
        if (p.StartsWith('/'))
            p = p[1..];
        return p.Trim();
    }
}
