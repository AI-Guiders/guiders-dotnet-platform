#nullable enable

namespace AIGuiders.Platform.CommandPlane;

/// <summary>Longest-prefix slash catalog (bundled + overlay merge). ADR-0153.</summary>
public sealed class CommandCatalogIndex
{
    readonly Dictionary<string, CatalogRouteEntry> _byPath;
    readonly string[] _pathsLongestFirst;

    CommandCatalogIndex(Dictionary<string, CatalogRouteEntry> byPath, string[] pathsLongestFirst)
    {
        _byPath = byPath;
        _pathsLongestFirst = pathsLongestFirst;
    }

    public static CommandCatalogIndex Empty { get; } = new(new(StringComparer.OrdinalIgnoreCase), []);

    public static CommandCatalogIndex FromDescriptors(IEnumerable<CommandDescriptor> descriptors)
    {
        var byPath = new Dictionary<string, CatalogRouteEntry>(StringComparer.OrdinalIgnoreCase);
        foreach (var d in descriptors)
        {
            foreach (var path in d.AllPaths())
            {
                var normalized = NormalizePath(path);
                if (normalized.Length == 0 || byPath.ContainsKey(normalized))
                    continue;
                byPath[normalized] = CatalogRouteEntry.FromDescriptor(d, normalized);
            }
        }

        return FromPathDictionary(byPath);
    }

    public static CommandCatalogIndex FromEntries(IEnumerable<CatalogRouteEntry> entries)
    {
        var byPath = new Dictionary<string, CatalogRouteEntry>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in entries)
        {
            var normalized = NormalizePath(entry.Path);
            if (normalized.Length == 0)
                continue;
            byPath[normalized] = entry with { Path = normalized };
        }

        return FromPathDictionary(byPath);
    }

    public IReadOnlyCollection<CatalogRouteEntry> Routes => _byPath.Values;

    static CommandCatalogIndex FromPathDictionary(Dictionary<string, CatalogRouteEntry> byPath)
    {
        var longest = byPath.Keys.OrderByDescending(p => p.Length).ThenBy(p => p, StringComparer.OrdinalIgnoreCase).ToArray();
        return new CommandCatalogIndex(byPath, longest);
    }

    public CommandCatalogIndex Merge(CommandCatalogIndex overlay)
    {
        var merged = new Dictionary<string, CatalogRouteEntry>(_byPath, StringComparer.OrdinalIgnoreCase);
        foreach (var (k, v) in overlay._byPath)
            merged.TryAdd(k, v);
        var longest = merged.Keys.OrderByDescending(p => p.Length).ThenBy(p => p, StringComparer.OrdinalIgnoreCase).ToArray();
        return new CommandCatalogIndex(merged, longest);
    }

    public bool TryGet(string slashPath, out CatalogRouteEntry entry) =>
        _byPath.TryGetValue(NormalizePath(slashPath), out entry);

    public bool TryResolveLongestPrefix(
        IReadOnlyList<string> tokens,
        bool endsWithSpace,
        out string canonicalPath,
        out string argTail,
        out bool isExactPath,
        out bool endsWithSpaceAfterPath,
        out CatalogRouteEntry entry)
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
