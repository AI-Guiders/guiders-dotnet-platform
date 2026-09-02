#nullable enable

using AIGuiders.Platform.Execution.Configurations.Workspace;

namespace AIGuiders.Platform.Execution.Documentation.Correspondence;

/// <summary>
/// Standalone L1 correspondence from <c>.cascade/workspace.toml</c> (ADR 0061 / 0155 / 0156).
/// Forward: path → feature + ADR docs. Reverse: explicit <c>code_anchors</c> + doc body scan.
/// </summary>
public static class CorrespondenceResolver
{
    public static string? FindWorkspaceRoot(string? startPath, string? hintRoot = null)
    {
        foreach (var candidate in CandidateRoots(startPath, hintRoot))
        {
            var toml = WorkspaceSources.CascadeTomlPath(candidate);
            if (File.Exists(toml))
                return candidate;
        }

        return null;
    }

    public static CorrespondenceResult? TryResolve(string absoluteFilePath, string? workspaceRootHint = null)
    {
        if (string.IsNullOrWhiteSpace(absoluteFilePath))
            return null;

        string abs;
        try { abs = Path.GetFullPath(absoluteFilePath.Trim()); }
        catch { return null; }

        var root = FindWorkspaceRoot(abs, workspaceRootHint);
        if (root is null)
            return null;

        var tomlPath = WorkspaceSources.CascadeTomlPath(root);
        var doc = WorkspaceSources.TryLoadCascade(root);
        if (doc is null)
            return null;

        var rel = CorrespondencePaths.TryRel(root, abs);
        if (rel is null)
            return null;

        var forward = WorkspaceForwardMap.Resolve(doc, root, rel);
        var reverse = DocReverseAnchorResolver.ResolveFromToml(doc, root, forward.DocPaths, rel);
        var layers = new List<string>(4);
        if (!string.IsNullOrWhiteSpace(forward.FeatureLine)) layers.Add("L1p");
        if (forward.ForwardDocs.Length > 0) layers.Add("L1");
        if (reverse.Length > 0) layers.Add("L1r");

        return new CorrespondenceResult(
            root,
            rel,
            forward.FeatureLine,
            forward.FeatureDocs,
            forward.AdrLine,
            forward.ForwardDocs,
            reverse,
            layers.ToArray(),
            tomlPath);
    }

    /// <summary>Unified correspondence context (ADR 0156 get_correspondence_context shape).</summary>
    public static object BuildContext(CorrespondenceResult result) => new
    {
        file = result.FileRel,
        activeLayers = result.ActiveLayers,
        layersBadge = string.Join(" · ", result.ActiveLayers),
        feature = result.FeatureLine is null
            ? null
            : new { line = result.FeatureLine, docs = result.FeatureDocs },
        forwardDocs = result.ForwardDocs
            .Select(d => new { path = d.Path, title = d.Title })
            .ToArray(),
        reverseAnchors = result.ReverseAnchors
            .Select(r => new
            {
                docPath = r.DocPath,
                docTitle = r.DocTitle,
                provenance = r.Provenance,
                kind = r.Kind,
                codeAnchor = new
                {
                    file = r.File,
                    lineStart = r.LineStart,
                    lineEnd = r.LineEnd,
                    memberKey = r.MemberKey,
                    wire = r.Wire
                },
                excerpt = r.Excerpt,
                docLineHint = r.DocLineHint
            })
            .ToArray()
    };

    static IEnumerable<string> CandidateRoots(string? startPath, string? hintRoot)
    {
        if (!string.IsNullOrWhiteSpace(hintRoot))
        {
            string h;
            try { h = Path.GetFullPath(hintRoot.Trim()); }
            catch { h = hintRoot.Trim(); }
            yield return h;
        }

        if (string.IsNullOrWhiteSpace(startPath))
            yield break;

        string cur;
        try
        {
            cur = File.Exists(startPath)
                ? Path.GetDirectoryName(Path.GetFullPath(startPath)) ?? ""
                : Path.GetFullPath(startPath);
        }
        catch
        {
            yield break;
        }

        while (!string.IsNullOrWhiteSpace(cur))
        {
            yield return cur;
            var parent = Path.GetDirectoryName(cur);
            if (string.IsNullOrWhiteSpace(parent) || string.Equals(parent, cur, StringComparison.OrdinalIgnoreCase))
                yield break;
            cur = parent;
        }
    }
}
