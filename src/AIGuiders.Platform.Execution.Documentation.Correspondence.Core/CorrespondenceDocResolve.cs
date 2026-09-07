#nullable enable

using AIGuiders.Platform.Modeling.Paths;

namespace AIGuiders.Platform.Execution.Documentation.Correspondence;

/// <summary>
/// Workspace-family doc resolution (forum 003 / FTC case, GUIDERS-ADR-0050).
/// A logical doc path is own-repo relative (<c>docs/adr/…</c>) or sibling-anchored
/// (<c>{siblingDir}/{rest}</c>). Own wins; otherwise ancestors of the workspace root
/// are probed — honest candidates on failure, never a silent miss.
/// </summary>
public static class CorrespondenceDocResolve
{
    public sealed record Resolved(string Logical, string? Abs, string Kind, IReadOnlyList<string> Candidates);

    public static Resolved TryResolve(string workspaceRoot, string docRaw)
    {
        var candidates = new List<string>();

        // Physical form first: rooted or root-relative path that exists on disk.
        var raw = (docRaw ?? string.Empty).Trim().Replace('\\', '/');
        if (raw.Length > 0 && Path.IsPathRooted(docRaw!.Trim()))
        {
            string absPhysical;
            try { absPhysical = Path.GetFullPath(docRaw.Trim()); }
            catch { return new Resolved(raw, null, "unresolved", candidates); }
            var logical = ToLogicalForm(workspaceRoot, absPhysical);
            if (logical is null)
                return new Resolved(raw, null, "outside_family", candidates);
            return new Resolved(logical, absPhysical,
                string.Equals(CorrespondencePaths.TryRel(workspaceRoot, absPhysical), logical, StringComparison.OrdinalIgnoreCase) ? "own" : "sibling",
                candidates);
        }

        var logical1 = LogicalPath.Normalize(raw);
        if (logical1.Length == 0)
            return new Resolved(raw, null, "empty", candidates);

        var ownAbs = Path.Combine(workspaceRoot, logical1.Replace('/', Path.DirectorySeparatorChar));
        candidates.Add(ownAbs);
        if (File.Exists(ownAbs))
            return new Resolved(logical1, ownAbs, "own", candidates);

        var slash = logical1.IndexOf('/');
        if (slash > 0)
        {
            string? root = workspaceRoot;
            while (!string.IsNullOrWhiteSpace(root))
            {
                var candidate = Path.Combine(root, logical1.Replace('/', Path.DirectorySeparatorChar));
                candidates.Add(candidate);
                if (File.Exists(candidate))
                    return new Resolved(logical1, candidate, "sibling", candidates);
                var parent = Path.GetDirectoryName(root);
                if (string.IsNullOrWhiteSpace(parent) || string.Equals(parent, root, StringComparison.OrdinalIgnoreCase))
                    break;
                root = parent;
            }
        }

        return new Resolved(logical1, null, "unresolved", candidates);
    }

    /// <summary>Physical → canonical logical form: own-repo relative or <c>{siblingDir}/{rest}</c> (ADR-0050).</summary>
        /// <summary>Physical → canonical logical form: own-repo relative or <c>{siblingDir}/{rest}</c> (ADR-0050).</summary>
    public static string? ToLogicalForm(string workspaceRoot, string absoluteDocPath)
    {
        string abs;
        try { abs = Path.GetFullPath(absoluteDocPath.Trim()); }
        catch { return null; }

        // First ancestor where the doc is inside yields the canonical logical form:
        // root hit = own form; any ancestor hit = {siblingDir}/{rest} by construction.
        string? root = workspaceRoot;
        while (!string.IsNullOrWhiteSpace(root))
        {
            if (CorrespondencePaths.TryRel(root, abs) is { Length: > 0 } rel)
                return rel;
            var parent = Path.GetDirectoryName(root);
            if (string.IsNullOrWhiteSpace(parent) || string.Equals(parent, root, StringComparison.OrdinalIgnoreCase))
                break;
            root = parent;
        }

        return null;
    }
}
