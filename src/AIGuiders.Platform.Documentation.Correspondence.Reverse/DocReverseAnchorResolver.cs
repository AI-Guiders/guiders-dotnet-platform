#nullable enable

using System.Text.RegularExpressions;

namespace AIGuiders.Platform.Documentation.Correspondence;

/// <summary>Reverse anchors: scan ADR/KB bodies for code paths and brackets (ADR 0156 §2.3).</summary>
public static partial class DocReverseAnchorResolver
{
    public static IReadOnlyList<ReverseAnchor> Resolve(
        string? workspaceRoot,
        string? navigationAbsolutePath,
        IReadOnlyList<string> forwardDocRepoPaths,
        IReadOnlyList<ExplicitCodeAnchor>? explicitAnchors = null)
    {
        if (string.IsNullOrWhiteSpace(workspaceRoot) || string.IsNullOrWhiteSpace(navigationAbsolutePath))
            return [];

        var root = workspaceRoot.Trim();
        var rel = CorrespondencePaths.TryRel(root, navigationAbsolutePath);
        if (rel is null)
            return [];

        return ResolveFromRel(root, rel, forwardDocRepoPaths, explicitAnchors: explicitAnchors);
    }

    public static ReverseAnchor[] ResolveFromToml(
        WorkspaceTomlDoc? doc,
        string workspaceRoot,
        IReadOnlyList<string> forwardDocPaths,
        string fileRel) =>
        ResolveFromRel(workspaceRoot, fileRel, forwardDocPaths, doc).ToArray();

    static IReadOnlyList<ReverseAnchor> ResolveFromRel(
        string workspaceRoot,
        string fileRel,
        IReadOnlyList<string> forwardDocPaths,
        WorkspaceTomlDoc? doc = null,
        IReadOnlyList<ExplicitCodeAnchor>? explicitAnchors = null)
    {
        var fileNorm = CorrespondencePaths.NormalizePath(fileRel);
        var fileName = Path.GetFileName(fileNorm);
        var list = new List<ReverseAnchor>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var overrides = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var rows = doc?.Workspace?.Correspondence?.CodeAnchors;
        if (rows is { Count: > 0 })
        {
            foreach (var row in rows)
            {
                var docPath = CorrespondencePaths.NormalizeDoc(row.Doc ?? "");
                if (docPath.Length == 0)
                    continue;

                if (!TryParseAnchor(row, out var file, out var lineStart, out var lineEnd, out var member, out var wire))
                    continue;

                if (!CorrespondencePaths.PathsMatch(file, fileNorm, fileName))
                    continue;

                var kind = string.IsNullOrWhiteSpace(row.Kind) ? "documents" : row.Kind.Trim();
                overrides.Add($"{docPath}|{CorrespondencePaths.NormalizePath(file)}");
                AddReverse(
                    list,
                    seen,
                    docPath,
                    CorrespondencePaths.GuessTitle(docPath),
                    CorrespondenceProvenance.WorkspaceToml,
                    kind,
                    file,
                    lineStart,
                    lineEnd,
                    member,
                    wire,
                    lineStart,
                    null);
            }
        }

        if (explicitAnchors is { Count: > 0 })
        {
            foreach (var entry in explicitAnchors)
            {
                if (!CorrespondencePaths.PathsMatch(entry.File, fileNorm, fileName))
                    continue;

                var docPath = CorrespondencePaths.NormalizeDoc(entry.DocPath);
                overrides.Add($"{docPath}|{CorrespondencePaths.NormalizePath(entry.File)}");
                AddReverse(
                    list,
                    seen,
                    docPath,
                    CorrespondencePaths.GuessTitle(docPath),
                    entry.Provenance,
                    entry.Kind,
                    entry.File,
                    entry.LineStart,
                    entry.LineEnd,
                    entry.MemberKey,
                    CorrespondenceWire.Build(entry.File, entry.LineStart, entry.LineEnd, entry.MemberKey),
                    entry.LineStart,
                    null);
            }
        }

        foreach (var docRel in forwardDocPaths.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var absDoc = Path.Combine(workspaceRoot, docRel.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(absDoc))
                continue;

            string md;
            try { md = File.ReadAllText(absDoc); }
            catch { continue; }

            ScanDocBody(docRel, md, fileNorm, fileName, overrides, list, seen);
        }

        return list
            .OrderBy(x => x.DocPath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.DocLineHint ?? int.MaxValue)
            .ToList();
    }

    static void ScanDocBody(
        string docRel,
        string markdown,
        string fileNorm,
        string fileName,
        HashSet<string> overrides,
        List<ReverseAnchor> list,
        HashSet<string> seen)
    {
        var title = CorrespondencePaths.GuessTitle(docRel);

        foreach (Match m in BracketInProseRegex().Matches(markdown))
        {
            var bracket = m.Value;
            if (!CorrespondenceWire.TryParseBracket(bracket, out var file, out var ls, out var le, out var member))
                continue;
            if (!CorrespondencePaths.PathsMatch(file, fileNorm, fileName))
                continue;
            if (overrides.Contains($"{CorrespondencePaths.NormalizeDoc(docRel)}|{CorrespondencePaths.NormalizePath(file)}"))
                continue;

            var lineHint = CorrespondencePaths.LineNumberAt(markdown, m.Index);
            AddReverse(
                list,
                seen,
                CorrespondencePaths.NormalizeDoc(docRel),
                title,
                CorrespondenceProvenance.Bracket,
                "documents",
                file,
                ls,
                le,
                member,
                CorrespondenceWire.Build(file, ls, le, member),
                lineHint,
                CorrespondencePaths.ExcerptAt(markdown, lineHint));
        }

        foreach (Match m in BacktickPathRegex().Matches(markdown))
        {
            var path = CorrespondencePaths.NormalizePath(m.Groups["path"].Value);
            if (!CorrespondencePaths.LooksLikeCodePath(path) || !CorrespondencePaths.PathsMatch(path, fileNorm, fileName))
                continue;
            if (overrides.Contains($"{CorrespondencePaths.NormalizeDoc(docRel)}|{path}"))
                continue;

            var lineHint = CorrespondencePaths.LineNumberAt(markdown, m.Index);
            AddReverse(
                list,
                seen,
                CorrespondencePaths.NormalizeDoc(docRel),
                title,
                CorrespondenceProvenance.DocBody,
                "documents",
                path,
                null,
                null,
                null,
                CorrespondenceWire.Build(path, null, null, null),
                lineHint,
                CorrespondencePaths.ExcerptAt(markdown, lineHint));
        }

        foreach (Match m in MarkdownCodeLinkRegex().Matches(markdown))
        {
            var path = CorrespondencePaths.NormalizePath(m.Groups["path"].Value.Split('#', 2)[0]);
            if (!CorrespondencePaths.PathsMatch(path, fileNorm, fileName))
                continue;
            if (overrides.Contains($"{CorrespondencePaths.NormalizeDoc(docRel)}|{path}"))
                continue;

            var lineHint = CorrespondencePaths.LineNumberAt(markdown, m.Index);
            AddReverse(
                list,
                seen,
                CorrespondencePaths.NormalizeDoc(docRel),
                title,
                CorrespondenceProvenance.DocBody,
                "documents",
                path,
                null,
                null,
                null,
                CorrespondenceWire.Build(path, null, null, null),
                lineHint,
                CorrespondencePaths.ExcerptAt(markdown, lineHint));
        }

        foreach (Match m in FileLineRangeRegex().Matches(markdown))
        {
            var path = CorrespondencePaths.NormalizePath(m.Groups["path"].Value);
            if (!CorrespondencePaths.PathsMatch(path, fileNorm, fileName))
                continue;
            if (overrides.Contains($"{CorrespondencePaths.NormalizeDoc(docRel)}|{path}"))
                continue;

            int? ls = int.TryParse(m.Groups["start"].Value, out var s) ? s : null;
            int? le = m.Groups["end"].Success && int.TryParse(m.Groups["end"].Value, out var e) ? e : null;
            var lineHint = CorrespondencePaths.LineNumberAt(markdown, m.Index);
            AddReverse(
                list,
                seen,
                CorrespondencePaths.NormalizeDoc(docRel),
                title,
                CorrespondenceProvenance.DocBody,
                "documents",
                path,
                ls,
                le,
                null,
                CorrespondenceWire.Build(path, ls, le, null),
                lineHint,
                CorrespondencePaths.ExcerptAt(markdown, lineHint));
        }
    }

    static void AddReverse(
        List<ReverseAnchor> list,
        HashSet<string> seen,
        string docPath,
        string title,
        string provenance,
        string kind,
        string file,
        int? lineStart,
        int? lineEnd,
        string? member,
        string wire,
        int? docLineHint,
        string? excerpt)
    {
        var key = $"{docPath}|{file}|{lineStart}|{member}|{provenance}";
        if (!seen.Add(key))
            return;

        list.Add(new ReverseAnchor(
            docPath,
            title,
            provenance,
            kind,
            file,
            lineStart,
            lineEnd,
            member,
            wire,
            docLineHint,
            excerpt));
    }

    static bool TryParseAnchor(
        CodeAnchorToml row,
        out string file,
        out int? lineStart,
        out int? lineEnd,
        out string? member,
        out string wire)
    {
        file = "";
        lineStart = row.LineStart;
        lineEnd = row.LineEnd;
        member = string.IsNullOrWhiteSpace(row.MemberKey) ? null : row.MemberKey.Trim();
        wire = "";

        if (!string.IsNullOrWhiteSpace(row.Bracket))
        {
            if (!CorrespondenceWire.TryParseBracket(row.Bracket, out file, out var bls, out var ble, out var bm))
                return false;
            lineStart ??= bls;
            lineEnd ??= ble;
            member ??= bm;
        }
        else
        {
            file = CorrespondencePaths.NormalizePath(row.File ?? "");
            if (file.Length == 0)
                return false;
        }

        wire = CorrespondenceWire.Build(file, lineStart, lineEnd, member);
        return true;
    }

    [GeneratedRegex(@"`(?<path>[\w./\\-]+\.(?:cs|fs|vb|csx))`", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex BacktickPathRegex();

    [GeneratedRegex(@"\[(?<label>[^\]]+)\]\((?<path>[^)\s#]+\.(?:cs|fs|vb|csx)[^)]*)\)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex MarkdownCodeLinkRegex();

    [GeneratedRegex(@"(?<path>[\w./\\-]+\.(?:cs|fs|vb|csx)):(?<start>\d+)(?:\s*[-–]\s*(?<end>\d+))?", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex FileLineRangeRegex();

    [GeneratedRegex(@"\[F:[^\]]+\]", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex BracketInProseRegex();
}
