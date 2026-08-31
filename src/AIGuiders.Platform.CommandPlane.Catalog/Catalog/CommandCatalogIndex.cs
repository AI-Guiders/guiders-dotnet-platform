using AIGuiders.Platform.IntermediateRepresentation.Command;
#nullable enable

using AIGuiders.Platform.Catalog;

namespace AIGuiders.Platform.CommandPlane;

/// <summary>Longest-prefix slash catalog (bundled + overlay merge). ADR-0153.</summary>
public sealed class CommandCatalogIndex
{
    readonly CatalogIndex<string, CatalogRouteEntry> _index;

    CommandCatalogIndex(CatalogIndex<string, CatalogRouteEntry> index) =>
        _index = index;

    public static CommandCatalogIndex Empty { get; } = Wrap(
        CatalogIndex<string, CatalogRouteEntry>.Empty(StringComparer.OrdinalIgnoreCase));

    public static CommandCatalogIndex FromDescriptors(IEnumerable<CommandDescriptor> descriptors) =>
        Wrap(CatalogIndex<string, CatalogRouteEntry>.FromDescriptors(descriptors, CommandCatalogProfile.Instance));

    public static CommandCatalogIndex FromEntries(IEnumerable<CatalogRouteEntry> entries)
    {
        var byPath = new Dictionary<string, CatalogRouteEntry>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in entries)
        {
            var normalized = CommandCatalogProfile.NormalizePath(entry.Path);
            if (normalized.Length == 0)
                continue;
            byPath[normalized] = entry with { Path = normalized };
        }

        return Wrap(CatalogIndex<string, CatalogRouteEntry>.FromMap(
            byPath,
            StringComparer.OrdinalIgnoreCase));
    }

    public IReadOnlyCollection<CatalogRouteEntry> Routes => _index.Entries;

    public CommandCatalogIndex Merge(CommandCatalogIndex overlay) =>
        Wrap(_index.MergeShipFirst(overlay._index));

    public bool TryGet(string slashPath, out CatalogRouteEntry entry) =>
        _index.TryGet(CommandCatalogProfile.NormalizePath(slashPath), out entry);

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
            if (!_index.TryGet(candidate, out var route))
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

    static CommandCatalogIndex Wrap(CatalogIndex<string, CatalogRouteEntry> index) =>
        new(index);
}
