using AIGuiders.Platform.IntermediateRepresentation.Command;
#nullable enable
using System.Runtime.CompilerServices;

namespace AIGuiders.Platform.CommandPlane;

/// <summary>Domain → object → intent → arg step autocomplete (GUIDERS-ADR-0011 / ADR-0012).</summary>
public static class SlashStepCompletion
{
    enum CompletionStep
    {
        Domain,
        Object,
        Intent,
        Arg,
    }

    readonly record struct CompletionState(
        CompletionStep Step,
        string? Domain,
        string? Object,
        string PartialToken);

    static readonly ConditionalWeakTable<CommandCatalogIndex, Snapshot> Snapshots = new();

    public static IReadOnlyList<ArgCompletionItem> GetSuggestions(
        CommandCatalogIndex catalog,
        string typedBody) =>
        GetSuggestions(catalog, typedBody, suggestionBroker: null);

    public static IReadOnlyList<ArgCompletionItem> GetSuggestions(
        CommandCatalogIndex catalog,
        string typedBody,
        ICommandArgSuggestionBroker? suggestionBroker) =>
        GetSuggestions(catalog, ParseTokens(typedBody, out var endsWithSpace), endsWithSpace, typedBody, suggestionBroker);

    public static IReadOnlyList<ArgCompletionItem> GetSuggestions(
        CommandCatalogIndex catalog,
        IReadOnlyList<string> tokens,
        bool endsWithSpace,
        string typedBody) =>
        GetSuggestions(catalog, tokens, endsWithSpace, typedBody, suggestionBroker: null);

    public static IReadOnlyList<ArgCompletionItem> GetSuggestions(
        CommandCatalogIndex catalog,
        IReadOnlyList<string> tokens,
        bool endsWithSpace,
        string typedBody,
        ICommandArgSuggestionBroker? suggestionBroker)
    {
        if (SlashLineResolver.TryResolveBody(typedBody, catalog, out var line)
            && catalog.TryGet(line.CanonicalPath, out var route)
            && SlashArgCompletion.ShouldComplete(line, route))
        {
            var argItems = SlashArgCompletion.GetSuggestions(line, route, suggestionBroker);
            if (argItems.Count > 0 || line.ShouldHideSegmentSuggestions)
            {
                return argItems;
            }
        }

        if (SlashLineResolver.TryResolveBody(typedBody, catalog, out line) && line.ShouldHideSegmentSuggestions)
            return [];

        var snap = Snapshots.GetValue(catalog, BuildSnapshot);
        if (!snap.HasSemanticStructure)
            return GetFlatPathSuggestions(catalog, snap, tokens, endsWithSpace);

        var state = ResolveCompletionState(snap, tokens, endsWithSpace);
        return state.Step switch
        {
            CompletionStep.Domain => BuildDomainSuggestions(snap, state.PartialToken),
            CompletionStep.Object => BuildObjectSuggestions(snap, state.Domain!, state.PartialToken, tokens, endsWithSpace),
            CompletionStep.Intent => BuildIntentSuggestions(
                snap,
                catalog,
                state.Domain!,
                state.Object ?? "",
                state.PartialToken,
                tokens,
                endsWithSpace),
            CompletionStep.Arg => [],
            _ => [],
        };
    }

    public static bool TryResolveHierarchy(
        CommandCatalogIndex catalog,
        IReadOnlyList<string> tokens,
        bool endsWithSpace,
        out CatalogSemanticFields fields,
        out string matchedPath)
    {
        fields = default;
        matchedPath = "";
        if (tokens.Count == 0)
            return false;

        var snap = Snapshots.GetValue(catalog, BuildSnapshot);
        if (!TryMatchPrefixByPath(snap, tokens, endsWithSpace, out matchedPath))
            return false;

        if (catalog.TryGet(matchedPath, out var route))
            fields = route.SemanticFields;
        else
            return false;

        return true;
    }

    static IReadOnlyList<string> ParseTokens(string typedBody, out bool endsWithSpace)
    {
        SlashLineResolver.ParseTypedBody(typedBody, out var tokens, out endsWithSpace);
        return tokens;
    }

    static CompletionState ResolveCompletionState(Snapshot snap, IReadOnlyList<string> tokens, bool endsWithSpace)
    {
        if (tokens.Count == 0)
            return new(CompletionStep.Domain, null, null, "");

        if (!endsWithSpace)
        {
            if (tokens.Count == 1)
            {
                var t = tokens[0];
                if (snap.DomainsWithCanonicalPrefix.Contains(t))
                    return new(CompletionStep.Object, t, null, "");

                if (snap.ElisionObjectToDomain.TryGetValue(t, out var elisionDomain))
                    return new(CompletionStep.Intent, elisionDomain, t, "");

                return new(CompletionStep.Domain, null, null, t);
            }

            if (TryResolvePrefix(snap, PrefixTokens(tokens, 1), endsWithSpace: true, out var domain, out var obj)
                && !string.IsNullOrEmpty(obj))
                return new(CompletionStep.Intent, domain, obj, tokens[^1]);

            if (tokens.Count >= 2
                && snap.DomainsWithCanonicalPrefix.Contains(tokens[0])
                && TryResolvePrefix(snap, [tokens[0]], endsWithSpace: true, out var domainOnly, out var emptyObj)
                && string.IsNullOrEmpty(emptyObj))
            {
                return tokens.Count == 2
                    ? new(CompletionStep.Object, domainOnly, null, tokens[1])
                    : new(CompletionStep.Intent, domainOnly, "", tokens[^1]);
            }

            return new(CompletionStep.Domain, null, null, tokens[^1]);
        }

        if (tokens.Count == 1)
        {
            var t0 = tokens[0];
            if (snap.DomainsWithCanonicalPrefix.Contains(t0))
                return new(CompletionStep.Object, t0, null, "");

            if (snap.ElisionObjectToDomain.TryGetValue(t0, out var elisionDomain))
                return new(CompletionStep.Intent, elisionDomain, t0, "");

            return new(CompletionStep.Domain, null, null, "");
        }

        if (tokens.Count == 2 && TryResolvePrefix(snap, tokens, endsWithSpace: true, out var d2, out var o2))
        {
            if (!string.IsNullOrEmpty(o2))
                return new(CompletionStep.Intent, d2, o2, "");

            return new(CompletionStep.Arg, d2, "", "");
        }

        if (tokens.Count >= 3 && TryResolvePrefix(snap, tokens, endsWithSpace: true, out var d3, out var o3))
            return new(CompletionStep.Arg, d3, o3, "");

        return new(CompletionStep.Arg, null, null, "");
    }

    static bool TryResolvePrefix(
        Snapshot snap,
        IReadOnlyList<string> tokens,
        bool endsWithSpace,
        out string? domain,
        out string? obj)
    {
        domain = null;
        obj = null;
        if (tokens.Count == 0)
            return false;

        var t0 = tokens[0];

        if (snap.ElisionObjectToDomain.TryGetValue(t0, out var elisionDomain))
        {
            domain = elisionDomain;
            obj = t0;
            if (tokens.Count == 1)
                return true;

            return endsWithSpace;
        }

        if (!snap.DomainsWithCanonicalPrefix.Contains(t0))
            return false;

        domain = t0;
        if (tokens.Count == 1)
        {
            obj = "";
            return true;
        }

        obj = tokens[1];
        return true;
    }

    static IReadOnlyList<ArgCompletionItem> BuildDomainSuggestions(Snapshot snap, string partial)
    {
        var buckets = new Dictionary<string, ArgCompletionItem>(StringComparer.OrdinalIgnoreCase);

        foreach (var domain in snap.DomainsWithCanonicalPrefix)
        {
            if (!MatchesPartial(domain, partial))
                continue;

            AddSuggestion(
                buckets,
                domain,
                $"/{domain} ",
                $"/{domain}",
                snap.BestHelpForDomain(domain));
        }

        foreach (var (starter, elisionDomain) in snap.ElisionObjectToDomain)
        {
            if (!MatchesPartial(starter, partial))
                continue;

            AddSuggestion(
                buckets,
                starter,
                $"/{starter} ",
                $"/{starter}",
                snap.BestHelpForElisionStarter(starter, elisionDomain));
        }

        return SlashCompletionSort.Order(buckets.Values);
    }

    static IReadOnlyList<ArgCompletionItem> BuildObjectSuggestions(
        Snapshot snap,
        string domain,
        string partial,
        IReadOnlyList<string> tokens,
        bool endsWithSpace)
    {
        var buckets = new Dictionary<string, ArgCompletionItem>(StringComparer.OrdinalIgnoreCase);

        if (snap.ObjectsByDomain.TryGetValue(domain, out var objects))
        {
            foreach (var obj in objects)
            {
                if (string.IsNullOrEmpty(obj) || !MatchesPartial(obj, partial))
                    continue;

                var insertPath = $"/{domain} {obj} ";
                AddSuggestion(
                    buckets,
                    obj,
                    insertPath,
                    insertPath.TrimEnd(),
                    snap.BestHelpForObject(domain, obj));
            }
        }

        if (snap.FlatIntentsByDomain.TryGetValue(domain, out var flatIntents))
        {
            foreach (var (intent, route) in flatIntents)
            {
                if (!MatchesPartial(intent, partial))
                    continue;

                var pathSegs = route.PathSegments;
                var insert = BuildInsertFromTyped(catalog: null, tokens, endsWithSpace, pathSegs, pathSegs.Count - 1, intent);
                AddSuggestion(buckets, intent, insert, route.CommandPath, route.Help, route.Group);
            }
        }

        return SlashCompletionSort.Order(buckets.Values);
    }

    static IReadOnlyList<ArgCompletionItem> BuildIntentSuggestions(
        Snapshot snap,
        CommandCatalogIndex catalog,
        string domain,
        string obj,
        string partial,
        IReadOnlyList<string> tokens,
        bool endsWithSpace)
    {
        var key = (domain, obj);
        if (!snap.RoutesBySemantic.TryGetValue(key, out var routes))
            return [];

        var buckets = new Dictionary<string, ArgCompletionItem>(StringComparer.OrdinalIgnoreCase);
        var segmentIndex = endsWithSpace ? tokens.Count : Math.Max(0, tokens.Count - 1);
        foreach (var route in routes)
        {
            var pathSegs = route.PathSegments;
            if (segmentIndex >= pathSegs.Count)
                continue;

            if (!PathPrefixMatches(pathSegs, tokens, endsWithSpace))
                continue;

            var segmentValue = pathSegs[segmentIndex];
            if (!MatchesPartial(segmentValue, partial))
                continue;

            var insert = BuildInsertFromTyped(catalog, tokens, endsWithSpace, pathSegs, segmentIndex, segmentValue);
            AddSuggestion(buckets, segmentValue, insert, route.CommandPath, route.Help, route.Group);
        }

        return SlashCompletionSort.Order(buckets.Values);
    }

    static IReadOnlyList<ArgCompletionItem> GetFlatPathSuggestions(
        CommandCatalogIndex catalog,
        Snapshot snap,
        IReadOnlyList<string> tokens,
        bool endsWithSpace)
    {
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
                continue;

            if (!PrefixMatches(segs, prefixTokens))
                continue;

            var next = segs[depth];
            if (partial.Length > 0 && !next.StartsWith(partial, StringComparison.OrdinalIgnoreCase))
                continue;

            if (!seen.Add(next))
                continue;

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

    static void AddSuggestion(
        Dictionary<string, ArgCompletionItem> buckets,
        string listTitle,
        string insert,
        string slashPath,
        string help,
        string? group = null)
    {
        if (!buckets.TryGetValue(listTitle, out var existing)
            || slashPath.Length > existing.CommandPath.Length)
        {
            buckets[listTitle] = new ArgCompletionItem(insert, slashPath, help, group, listTitle);
        }
    }

    static bool MatchesPartial(string value, string partial) =>
        partial.Length == 0
        || value.StartsWith(partial, StringComparison.OrdinalIgnoreCase);

    static string BuildInsertFromTyped(
        CommandCatalogIndex? catalog,
        IReadOnlyList<string> typedTokens,
        bool endsWithSpace,
        IReadOnlyList<string> pathSegs,
        int completeSegmentIndex,
        string segmentValue)
    {
        var resultSegs = new List<string>(completeSegmentIndex + 1);
        for (var i = 0; i < completeSegmentIndex; i++)
            resultSegs.Add(i < typedTokens.Count ? typedTokens[i] : pathSegs[i]);

        resultSegs.Add(segmentValue);
        var slashPath = "/" + string.Join(" ", resultSegs);
        if (completeSegmentIndex + 1 < pathSegs.Count
            || (catalog is not null && SegmentNeedsArgTail(catalog, slashPath)))
            slashPath += " ";

        return slashPath;
    }

    static bool SegmentNeedsArgTail(CommandCatalogIndex catalog, string slashPath)
    {
        if (SlashLineResolver.TryResolveSlashLine(slashPath, catalog, out var line)
            && line.IsExactPathMatch
            && line.ArgTailKind == CommandArgTailKind.None)
            return false;

        return SlashLineResolver.TryResolveSlashLine(slashPath, catalog, out var resolved)
               && resolved.ArgTailKind != CommandArgTailKind.None;
    }

    static bool PathPrefixMatches(
        IReadOnlyList<string> pathSegs,
        IReadOnlyList<string> tokens,
        bool endsWithSpace)
    {
        if (tokens.Count == 0)
            return true;

        if (endsWithSpace)
        {
            if (tokens.Count >= pathSegs.Count)
                return false;

            for (var i = 0; i < tokens.Count; i++)
            {
                if (!pathSegs[i].Equals(tokens[i], StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            return true;
        }

        if (tokens.Count > pathSegs.Count)
            return false;

        for (var i = 0; i < tokens.Count - 1; i++)
        {
            if (!pathSegs[i].Equals(tokens[i], StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return pathSegs[tokens.Count - 1].StartsWith(tokens[^1], StringComparison.OrdinalIgnoreCase);
    }

    static bool PrefixMatches(IReadOnlyList<string> segs, IReadOnlyList<string> prefixTokens)
    {
        if (prefixTokens.Count > segs.Count)
            return false;

        for (var i = 0; i < prefixTokens.Count; i++)
        {
            if (!segs[i].Equals(prefixTokens[i], StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }

    static bool TryMatchPrefixByPath(
        Snapshot snap,
        IReadOnlyList<string> tokens,
        bool endsWithSpace,
        out string matchedPath)
    {
        matchedPath = "";
        var bestLen = -1;

        foreach (var route in snap.AllRoutes)
        {
            if (!PathPrefixMatches(route.PathSegments, tokens, endsWithSpace))
                continue;

            if (route.PathSegments.Count <= bestLen)
                continue;

            bestLen = route.PathSegments.Count;
            matchedPath = route.CommandPath;
        }

        return bestLen >= 0;
    }

    static Snapshot BuildSnapshot(CommandCatalogIndex catalog)
    {
        var allRoutes = new List<IndexedRoute>();
        var domainsWithCanonicalPrefix = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var elisionObjectToDomain = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var objectsByDomain = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        var flatIntentsByDomain = new Dictionary<string, Dictionary<string, IndexedRoute>>(StringComparer.OrdinalIgnoreCase);
        var routesBySemantic = new Dictionary<(string Domain, string Object), List<IndexedRoute>>();

        var helpDomain = new Dictionary<string, (string Help, int Len)>(StringComparer.OrdinalIgnoreCase);
        var helpElision = new Dictionary<string, (string Help, int Len)>(StringComparer.OrdinalIgnoreCase);
        var helpObject = new Dictionary<(string, string), (string Help, int Len)>();

        foreach (var route in catalog.Routes)
        {
            var sem = route.SemanticFields;
            var pathSegs = SplitPath(route.Path);
            if (pathSegs.Count == 0)
                continue;

            var indexed = new IndexedRoute(route, sem, pathSegs);
            allRoutes.Add(indexed);

            var domain = sem.Domain;
            var obj = sem.Object ?? "";
            var key = (domain, obj);

            if (!routesBySemantic.TryGetValue(key, out var list))
            {
                list = [];
                routesBySemantic[key] = list;
            }

            list.Add(indexed);

            if (sem.DomainOmittedInPath && !string.IsNullOrEmpty(obj))
                elisionObjectToDomain[obj] = domain;
            else if (!string.IsNullOrEmpty(domain))
                domainsWithCanonicalPrefix.Add(domain);

            if (!objectsByDomain.TryGetValue(domain, out var objects))
            {
                objects = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                objectsByDomain[domain] = objects;
            }

            if (!string.IsNullOrEmpty(obj))
                objects.Add(obj);

            if (string.IsNullOrEmpty(obj) && !string.IsNullOrEmpty(sem.Intent))
            {
                if (!flatIntentsByDomain.TryGetValue(domain, out var flat))
                {
                    flat = new Dictionary<string, IndexedRoute>(StringComparer.OrdinalIgnoreCase);
                    flatIntentsByDomain[domain] = flat;
                }

                if (!flat.TryGetValue(sem.Intent, out var existing)
                    || route.Path.Length > existing.CommandPath.Length)
                    flat[sem.Intent] = indexed;
            }

            TrackHelp(helpDomain, domain, route);
            if (sem.DomainOmittedInPath && !string.IsNullOrEmpty(obj))
                TrackHelp(helpElision, obj, route);
            if (!string.IsNullOrEmpty(obj))
                TrackHelp(helpObject, (domain, obj), route);
        }

        return new Snapshot(
            allRoutes,
            domainsWithCanonicalPrefix,
            elisionObjectToDomain,
            objectsByDomain,
            flatIntentsByDomain,
            routesBySemantic,
            helpDomain,
            helpElision,
            helpObject);
    }

    static void TrackHelp<T>(
        Dictionary<T, (string Help, int Len)> map,
        T key,
        CatalogRouteEntry route)
        where T : notnull
    {
        if (!map.TryGetValue(key, out var existing) || route.Path.Length > existing.Len)
            map[key] = (route.Help, route.Path.Length);
    }

    static IReadOnlyList<string> PrefixTokens(IReadOnlyList<string> tokens, int dropLast)
    {
        if (dropLast <= 0)
            return tokens;

        var count = tokens.Count - dropLast;
        if (count <= 0)
            return [];

        var result = new List<string>(count);
        for (var i = 0; i < count; i++)
            result.Add(tokens[i]);

        return result;
    }

    static List<string> SplitPath(string slashPath)
    {
        var path = slashPath.Trim();
        if (path.StartsWith('/'))
            path = path[1..];

        return path.Length == 0
            ? []
            : path.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    sealed record IndexedRoute(
        CatalogRouteEntry Route,
        CatalogSemanticFields Semantics,
        List<string> PathSegments)
    {
        public string CommandPath => "/" + string.Join(' ', PathSegments);
        public string Help => Route.Help;
        public string? Group => Route.Group;
    }

    sealed class Snapshot
    {
        public Snapshot(
            List<IndexedRoute> allRoutes,
            HashSet<string> domainsWithCanonicalPrefix,
            Dictionary<string, string> elisionObjectToDomain,
            Dictionary<string, HashSet<string>> objectsByDomain,
            Dictionary<string, Dictionary<string, IndexedRoute>> flatIntentsByDomain,
            Dictionary<(string Domain, string Object), List<IndexedRoute>> routesBySemantic,
            Dictionary<string, (string Help, int Len)> helpDomain,
            Dictionary<string, (string Help, int Len)> helpElision,
            Dictionary<(string, string), (string Help, int Len)> helpObject)
        {
            AllRoutes = allRoutes;
            DomainsWithCanonicalPrefix = domainsWithCanonicalPrefix;
            ElisionObjectToDomain = elisionObjectToDomain;
            ObjectsByDomain = objectsByDomain;
            FlatIntentsByDomain = flatIntentsByDomain;
            RoutesBySemantic = routesBySemantic;
            _helpDomain = helpDomain;
            _helpElision = helpElision;
            _helpObject = helpObject;
        }

        public List<IndexedRoute> AllRoutes { get; }
        public HashSet<string> DomainsWithCanonicalPrefix { get; }
        public Dictionary<string, string> ElisionObjectToDomain { get; }
        public Dictionary<string, HashSet<string>> ObjectsByDomain { get; }
        public Dictionary<string, Dictionary<string, IndexedRoute>> FlatIntentsByDomain { get; }
        public Dictionary<(string Domain, string Object), List<IndexedRoute>> RoutesBySemantic { get; }

        public bool HasSemanticStructure =>
            DomainsWithCanonicalPrefix.Count > 0 || ElisionObjectToDomain.Count > 0;

        readonly Dictionary<string, (string Help, int Len)> _helpDomain;
        readonly Dictionary<string, (string Help, int Len)> _helpElision;
        readonly Dictionary<(string, string), (string Help, int Len)> _helpObject;

        public string BestHelpForDomain(string domain) =>
            _helpDomain.TryGetValue(domain, out var h) ? h.Help : domain;

        public string BestHelpForElisionStarter(string starter, string domain) =>
            _helpElision.TryGetValue(starter, out var h) ? h.Help : $"{starter} ({domain})";

        public string BestHelpForObject(string domain, string obj) =>
            _helpObject.TryGetValue((domain, obj), out var h) ? h.Help : obj;
    }
}
