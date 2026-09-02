#nullable enable

using System.Text.RegularExpressions;
using AIGuiders.Platform.Modeling.Paths;

namespace AIGuiders.Platform.Execution.Documentation.Correspondence;

public static partial class WorkspaceForwardMap
{
    public static ForwardMapResult Resolve(WorkspaceDocument? doc, string workspaceRoot, string fileRel)
    {
        var feature = ResolveFeature(doc, fileRel);
        var featureLine = BuildFeatureLine(feature);
        var featureDocs = feature?.Docs?
            .Select(CorrespondencePaths.NormalizeDoc)
            .Where(static d => d.Length > 0)
            .ToArray() ?? [];

        var docs = new List<string>(featureDocs);
        foreach (var m in ResolveAdrMap(doc, fileRel))
        {
            if (!docs.Contains(m, StringComparer.OrdinalIgnoreCase))
                docs.Add(m);
        }

        var auto = NormalizeAutoInclude(doc?.Workspace?.Adr?.AutoInclude);
        var maxRelated = doc?.Workspace?.Adr?.MaxRelated is int mr && mr > 0 ? mr : 8;
        if (auto == "linked" && docs.Count > 0)
        {
            var primary = docs[0];
            var absPrimary = Path.Combine(workspaceRoot, primary.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(absPrimary))
            {
                var linked = ExtractLinkedAdrs(
                    File.ReadAllText(absPrimary),
                    primary,
                    NormalizeAdrRoot(doc?.Workspace?.Adr?.RootDir));
                var baseCount = docs.Count;
                foreach (var l in linked)
                {
                    if (docs.Count >= baseCount + maxRelated)
                        break;
                    if (docs.Contains(l, StringComparer.OrdinalIgnoreCase))
                        continue;
                    docs.Add(l);
                }
            }
        }

        var forward = docs
            .Select(p => new ForwardDoc(p, CorrespondencePaths.GuessTitle(p)))
            .ToArray();

        return new ForwardMapResult(
            string.IsNullOrWhiteSpace(featureLine) ? null : featureLine,
            featureDocs,
            BuildAdrLine(docs),
            docs,
            forward);
    }

    static WorkspaceFeature? ResolveFeature(WorkspaceDocument? doc, string rel)
    {
        var features = doc?.Workspace?.Features?.Feature;
        if (features is not { Count: > 0 })
            return null;

        var normalized = CorrespondencePaths.NormalizePath(rel);
        WorkspaceFeature? best = null;
        var bestLen = -1;
        foreach (var f in features)
        {
            if (f.Paths is not { Count: > 0 })
                continue;
            foreach (var raw in f.Paths)
            {
                var p = CorrespondencePaths.NormalizePath(raw);
                if (p.Length == 0)
                    continue;
                if (!normalized.StartsWith(p, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (p.Length > bestLen)
                {
                    best = f;
                    bestLen = p.Length;
                }
            }
        }

        return best;
    }

    static string BuildFeatureLine(WorkspaceFeature? feature)
    {
        if (feature is null) return "";
        var title = (feature.Title ?? "").Trim();
        var id = (feature.Id ?? "").Trim();
        if (title.Length > 0 && id.Length > 0) return $"Feature: {title} ({id})";
        if (title.Length > 0) return $"Feature: {title}";
        if (id.Length > 0) return $"Feature: {id}";
        return "";
    }

    static List<string> ResolveAdrMap(WorkspaceDocument? doc, string rel)
    {
        var map = doc?.Workspace?.Adr?.Map;
        if (map is not { Count: > 0 })
            return [];

        var normalized = CorrespondencePaths.NormalizePath(rel);
        string? bestKey = null;
        var bestLen = -1;
        foreach (var rawKey in map.Keys)
        {
            var k = CorrespondencePaths.NormalizePath(rawKey);
            if (k == "*")
            {
                if (bestKey is null)
                {
                    bestKey = rawKey;
                    bestLen = 0;
                }
                continue;
            }

            if (!normalized.StartsWith(k, StringComparison.OrdinalIgnoreCase))
                continue;
            if (k.Length > bestLen)
            {
                bestKey = rawKey;
                bestLen = k.Length;
            }
        }

        if (bestKey is null || !map.TryGetValue(bestKey, out var v))
            return [];

        return ExtractStrings(v)
            .Select(CorrespondencePaths.NormalizeDoc)
            .Where(static x => x.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    static IReadOnlyList<string> ExtractStrings(object? v)
    {
        if (v is null) return [];
        if (v is string s) return string.IsNullOrWhiteSpace(s) ? [] : [s.Trim()];
        if (v is IEnumerable<object> objs)
        {
            var list = new List<string>();
            foreach (var o in objs)
            {
                if (o is string os && os.Trim().Length > 0)
                    list.Add(os.Trim());
                else if (o is not null)
                {
                    var t = o.ToString();
                    if (!string.IsNullOrWhiteSpace(t))
                        list.Add(t.Trim());
                }
            }

            return list;
        }

        var asText = v.ToString();
        return string.IsNullOrWhiteSpace(asText) ? [] : [asText.Trim()];
    }

    static List<string> ExtractLinkedAdrs(string markdown, string currentDoc, string adrRoot)
    {
        var list = new List<string>();
        var current = CorrespondencePaths.NormalizeDoc(currentDoc);
        foreach (Match m in MdLinkRegex().Matches(markdown))
        {
            var raw = m.Groups["target"].Value.Trim();
            if (raw.Length == 0) continue;
            if (raw.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || raw.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                continue;

            var hash = raw.IndexOf('#');
            if (hash >= 0) raw = raw[..hash];
            if (raw.Length == 0) continue;

            string? resolved = null;
            var t = LogicalPath.Normalize(raw);
            if (t.StartsWith(adrRoot, StringComparison.OrdinalIgnoreCase))
                resolved = CorrespondencePaths.NormalizeDoc(t);
            else if (t.StartsWith("./", StringComparison.Ordinal)
                     || t.StartsWith("../", StringComparison.Ordinal)
                     || (!t.Contains(':') && !t.StartsWith('/')))
            {
                var lastSlash = current.LastIndexOf('/');
                var baseDir = lastSlash >= 0 ? current[..(lastSlash + 1)] : "";
                var parts = new List<string>();
                foreach (var p in (baseDir + t).Split('/', StringSplitOptions.RemoveEmptyEntries))
                {
                    if (p == ".") continue;
                    if (p == "..")
                    {
                        if (parts.Count > 0) parts.RemoveAt(parts.Count - 1);
                        continue;
                    }
                    parts.Add(p);
                }

                resolved = parts.Count == 0 ? null : string.Join('/', parts);
            }

            if (resolved is null) continue;
            if (!resolved.StartsWith(adrRoot, StringComparison.OrdinalIgnoreCase)) continue;
            if (string.Equals(resolved, current, StringComparison.OrdinalIgnoreCase)) continue;
            list.Add(resolved);
        }

        return list.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    static string BuildAdrLine(IReadOnlyList<string> docs)
    {
        if (docs.Count == 0) return "";
        var ids = docs.Select(CorrespondencePaths.GuessTitle).ToList();
        return ids.Count == 1 ? $"ADR: {ids[0]}" : $"ADR: {ids[0]} (+{ids.Count - 1})";
    }

    static string NormalizeAutoInclude(string? raw) =>
        string.Equals((raw ?? "").Trim(), "linked", StringComparison.OrdinalIgnoreCase) ? "linked" : "none";

    static string NormalizeAdrRoot(string? raw)
    {
        var s = LogicalPath.Normalize(raw ?? "");
        if (s.Length == 0) return "docs/adr/";
        if (!s.EndsWith('/')) s += "/";
        return s;
    }

    [GeneratedRegex(@"\[[^\]]*\]\((?<target>[^)]+)\)", RegexOptions.CultureInvariant)]
    private static partial Regex MdLinkRegex();
}
