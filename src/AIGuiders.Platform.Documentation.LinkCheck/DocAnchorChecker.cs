#nullable enable

using AIGuiders.Platform.Documentation.Anchors;
using AIGuiders.Platform.Notations.Bracket;

namespace AIGuiders.Platform.Documentation.LinkCheck;

/// <summary>Dry-resolve <c>Family:doc</c> bracket wires in markdown files.</summary>
public static class DocAnchorChecker
{
    public static IReadOnlyList<string> CheckMarkdownRoots(
        IEnumerable<string> rootPaths,
        IDocSymbolCatalog catalog)
    {
        var resolver = new DocSymbolAnchorResolver(catalog);
        var profile = BracketProfiles.DocSymbol;
        var failures = new List<string>();

        foreach (var path in rootPaths)
        {
            if (File.Exists(path) && path.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                CheckFile(path, profile, resolver, failures);
            else if (Directory.Exists(path))
            {
                foreach (var file in Directory.EnumerateFiles(path, "*.md", SearchOption.AllDirectories))
                    CheckFile(file, profile, resolver, failures);
            }
            else
                failures.Add($"{path}: not found");
        }

        return failures;
    }

    static void CheckFile(
        string filePath,
        BracketNotationProfile profile,
        DocSymbolAnchorResolver resolver,
        List<string> failures)
    {
        var text = File.ReadAllText(filePath);
        foreach (var envelope in BracketEnvelopeScan.LocateInText(text))
        {
            if (!LooksLikeDocAnchor(envelope.Inner))
                continue;

            if (!BracketReader.Default.TryRead(
                    envelope.Wire,
                    profile,
                    BracketAxisValuePlans.DocSymbol,
                    out var wire,
                    out var parseError) || wire is null)
            {
                failures.Add($"{filePath}:{LineNumber(text, envelope.Start)}: parse:{parseError}");
                continue;
            }

            if (!HasDocFamily(wire))
                continue;

            if (!resolver.TryResolve(wire, out var resolveError))
                failures.Add($"{filePath}:{LineNumber(text, envelope.Start)}: {resolveError} wire={envelope.Wire}");
        }
    }

    static int LineNumber(string text, int index) => text.Take(index).Count(c => c == '\n') + 1;

    static bool LooksLikeDocAnchor(string inner) =>
        inner.Contains("Family:", StringComparison.OrdinalIgnoreCase)
        && inner.Contains("doc", StringComparison.OrdinalIgnoreCase);

    static bool HasDocFamily(NormalizedBracketWire wire)
    {
        foreach (var axis in wire.Axes)
        {
            if (axis.Key.Equals("Family", StringComparison.OrdinalIgnoreCase)
                && axis.Value.Equals("doc", StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}
