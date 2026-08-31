#nullable enable

using AIGuiders.Platform.IntermediateRepresentation.Command;
using System.Runtime.CompilerServices;

namespace AIGuiders.Platform.CommandPlane;

/// <summary>Next-segment discovery from <see cref="CommandCatalogIndex"/> (GUIDERS-ADR-0046).</summary>
public static class CatalogPathCompletion
{
    static readonly ConditionalWeakTable<CommandCatalogIndex, PathSnapshot> Snapshots = new();

    public static IReadOnlyList<ArgCompletionItem> GetSuggestions(
        CommandCatalogIndex catalog,
        string typedBody) =>
        GetSuggestions(catalog, ParseTokens(typedBody, out var endsWithSpace), endsWithSpace, typedBody);

    public static IReadOnlyList<ArgCompletionItem> GetSuggestions(
        CommandCatalogIndex catalog,
        IReadOnlyList<string> tokens,
        bool endsWithSpace,
        string typedBody) =>
        GetFlatPathSuggestions(
            catalog,
            Snapshots.GetValue(catalog, BuildSnapshot),
            tokens,
            endsWithSpace,
            typedBody);

    internal static bool UsesFlatPaths(CommandCatalogIndex catalog) =>
        !Snapshots.GetValue(catalog, BuildSnapshot).HasSemanticStructure;

    static IReadOnlyList<string> ParseTokens(string typedBody, out bool endsWithSpace)
    {
        SlashLineResolver.ParseTypedBody(typedBody, out var tokens, out endsWithSpace);
        return tokens;
    }

    static IReadOnlyList<ArgCompletionItem> GetFlatPathSuggestions(
        CommandCatalogIndex catalog,
        PathSnapshot snap,
        IReadOnlyList<string> tokens,
        bool endsWithSpace,
        string typedBody)
    {
        if (!endsWithSpace && tokens.Count > 0 && HasChildSegments(snap, tokens))
        {
            endsWithSpace = true;
        }

        var depth = endsWithSpace ? tokens.Count : Math.Max(0, tokens.Count - 1);
        var partial = endsWithSpace || tokens.Count == 0 ? "" : tokens[^1];
        var prefixTokens = endsWithSpace
            ? tokens
            : tokens.Take(Math.Max(0, tokens.Count - 1)).ToArray();

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var list = new List<ArgCompletionItem>();

        foreach (var route in snap.AllRoutes)
        {
            var segs = route.PathSegments;
            if (segs.Count <= depth)
            {
                continue;
            }

            if (!PrefixMatches(segs, prefixTokens))
            {
                continue;
            }

            var next = segs[depth];
            if (partial.Length > 0 && !next.StartsWith(partial, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!seen.Add(next))
            {
                continue;
            }

            var insertSegs = prefixTokens.Concat([next]).ToArray();
            var slashPath = "/" + string.Join(' ', insertSegs);
            var more = segs.Count > depth + 1 || route.Route.ArgTailKind != CommandArgTailKind.None;
            var insert = slashPath + (more ? " " : "");
            var help = segs.Count == depth + 1
                ? route.Help
                : $"{route.CommandPath} — {route.Help}";
            list.Add(new ArgCompletionItem(insert, route.CommandPath, help, route.Group, next));
        }

        return SlashCompletionSort.Order(list);
    }

    static bool HasChildSegments(PathSnapshot snap, IReadOnlyList<string> tokens)
    {
        foreach (var route in snap.AllRoutes)
        {
            if (route.PathSegments.Count <= tokens.Count)
            {
                continue;
            }

            if (PrefixMatches(route.PathSegments, tokens))
            {
                return true;
            }
        }

        return false;
    }

    static bool PrefixMatches(IReadOnlyList<string> segs, IReadOnlyList<string> prefixTokens)
    {
        if (prefixTokens.Count > segs.Count)
        {
            return false;
        }

        for (var i = 0; i < prefixTokens.Count; i++)
        {
            if (!segs[i].Equals(prefixTokens[i], StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    static PathSnapshot BuildSnapshot(CommandCatalogIndex catalog)
    {
        var allRoutes = new List<IndexedRoute>();
        var domainsWithCanonicalPrefix = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var elisionObjectToDomain = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var route in catalog.Routes)
        {
            var sem = route.SemanticFields;
            var pathSegs = SplitPath(route.Path);
            if (pathSegs.Count == 0)
            {
                continue;
            }

            allRoutes.Add(new IndexedRoute(route, pathSegs));

            if (sem.DomainOmittedInPath && !string.IsNullOrEmpty(sem.Object))
            {
                elisionObjectToDomain[sem.Object] = sem.Domain;
            }
            else if (!string.IsNullOrEmpty(sem.Domain))
            {
                domainsWithCanonicalPrefix.Add(sem.Domain);
            }
        }

        return new PathSnapshot(
            allRoutes,
            domainsWithCanonicalPrefix.Count > 0 || elisionObjectToDomain.Count > 0);
    }

    static List<string> SplitPath(string slashPath)
    {
        var path = slashPath.Trim();
        if (path.StartsWith('/'))
        {
            path = path[1..];
        }

        return path.Length == 0
            ? []
            : path.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    sealed record IndexedRoute(CatalogRouteEntry Route, List<string> PathSegments)
    {
        public string CommandPath => "/" + string.Join(' ', PathSegments);
        public string Help => Route.Help;
        public string? Group => Route.Group;
    }

    sealed class PathSnapshot(List<IndexedRoute> allRoutes, bool hasSemanticStructure)
    {
        public List<IndexedRoute> AllRoutes { get; } = allRoutes;
        public bool HasSemanticStructure { get; } = hasSemanticStructure;
    }
}
