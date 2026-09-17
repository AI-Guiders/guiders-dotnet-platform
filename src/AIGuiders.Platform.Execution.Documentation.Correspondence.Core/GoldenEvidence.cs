using System.Text.RegularExpressions;

namespace AIGuiders.Platform.Execution.Documentation.Correspondence;

/// <summary>Golden session test evidence discovery (ADR-0006 / 0007). Execution IO scan — no Modeling File.*.</summary>
public static class GoldenEvidence
{
    static readonly string[] EvidencePatterns = ["*.fs", "*.cs"];

    static readonly Regex GoldenSessionAttributeRegex = new(
        @"\[GoldenSession\s*\(\s*""(?<id>[^""]+)""\s*\)\]",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    static readonly Regex GoldenColonRegex = new(
        @"\bgolden:\s*(?<id>[A-Za-z0-9_-]+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    static readonly Regex TestMarkerRegex = new(
        @"\[(Fact|Theory|Test|TestMethod)\b|member\s|TestMethod\s|GoldenSession\b|golden:",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public sealed record Found(string Path, string Hint);

    public static bool Exists(string workspaceRoot, string goldenId) =>
        TryLocate(workspaceRoot, goldenId, out _);

    public static bool TryLocate(string workspaceRoot, string goldenId, out Found found)
    {
        found = default!;
        if (string.IsNullOrWhiteSpace(workspaceRoot) || string.IsNullOrWhiteSpace(goldenId))
            return false;

        var root = Path.GetFullPath(workspaceRoot.Trim());
        Found? best = null;
        var bestStrength = -1;

        foreach (var file in EnumerateEvidenceFiles(root))
        {
            var content = File.ReadAllText(file);
            if (!TryDetectEvidence(file, content, goldenId, out var candidate, out var strength))
                continue;

            if (strength <= bestStrength)
                continue;

            bestStrength = strength;
            best = candidate;
        }

        if (best is null)
            return false;

        found = best;
        return true;
    }

    static IEnumerable<string> EnumerateEvidenceFiles(string workspaceRoot)
    {
        if (!Directory.Exists(workspaceRoot))
            yield break;

        foreach (var pattern in EvidencePatterns)
        {
            foreach (var file in Directory.EnumerateFiles(workspaceRoot, pattern, SearchOption.AllDirectories))
            {
                if (IsExcludedPath(file) || !LooksLikeTestFile(file))
                    continue;

                yield return file;
            }
        }
    }

    static bool IsExcludedPath(string path) =>
        path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
        || path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
        || path.Contains($"{Path.DirectorySeparatorChar}node_modules{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);

    static bool LooksLikeTestFile(string path) =>
        path.Contains("Tests", StringComparison.OrdinalIgnoreCase)
        || path.Contains(".Test.", StringComparison.OrdinalIgnoreCase)
        || path.EndsWith("Tests.cs", StringComparison.OrdinalIgnoreCase)
        || path.EndsWith("Tests.fs", StringComparison.OrdinalIgnoreCase);

    static bool TryDetectEvidence(
        string path,
        string content,
        string goldenId,
        out Found found,
        out int strength)
    {
        found = default!;
        strength = 0;

        var candidates = new List<(int Strength, Found Found)>();

        if (TryMatchGoldenSessionAttribute(content, goldenId, out var attrHint))
            candidates.Add((3, new Found(path, attrHint)));

        if (TryMatchGoldenColon(content, goldenId, out var colonHint))
            candidates.Add((3, new Found(path, colonHint)));

        if (TryMatchTestName(path, goldenId, out var nameHint))
            candidates.Add((2, new Found(path, nameHint)));

        if (TryMatchTestContext(content, goldenId, out var contextHint))
            candidates.Add((1, new Found(path, contextHint)));

        if (candidates.Count == 0)
            return false;

        var best = candidates.OrderByDescending(c => c.Strength).First();

        strength = best.Strength;
        found = best.Found;
        return true;
    }

    static bool TryMatchGoldenSessionAttribute(string content, string goldenId, out string hint)
    {
        hint = "";
        foreach (Match match in GoldenSessionAttributeRegex.Matches(content))
        {
            if (!match.Groups["id"].Value.Equals(goldenId, StringComparison.OrdinalIgnoreCase))
                continue;

            hint = "GoldenSession attribute";
            return true;
        }

        return false;
    }

    static bool TryMatchGoldenColon(string content, string goldenId, out string hint)
    {
        hint = "";
        foreach (Match match in GoldenColonRegex.Matches(content))
        {
            if (!match.Groups["id"].Value.Equals(goldenId, StringComparison.OrdinalIgnoreCase))
                continue;

            hint = "golden: marker";
            return true;
        }

        if (content.Contains($"golden:{goldenId}", StringComparison.OrdinalIgnoreCase))
        {
            hint = "golden: inline";
            return true;
        }

        if (content.Contains($"golden: {goldenId}", StringComparison.OrdinalIgnoreCase))
        {
            hint = "golden: inline";
            return true;
        }

        return false;
    }

    static bool TryMatchTestContext(string content, string goldenId, out string hint)
    {
        hint = "";
        if (!content.Contains(goldenId, StringComparison.OrdinalIgnoreCase))
            return false;

        if (!TestMarkerRegex.IsMatch(content))
            return false;

        hint = "test marker with golden id";
        return true;
    }

    static bool TryMatchTestName(string path, string goldenId, out string hint)
    {
        hint = "";
        var fileName = Path.GetFileNameWithoutExtension(path);
        if (string.IsNullOrEmpty(fileName))
            return false;

        if (!fileName.Contains(goldenId, StringComparison.OrdinalIgnoreCase))
            return false;

        hint = "test file name";
        return true;
    }
}
