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

        foreach (var path in ExpandPaths(rootPaths))
        {
            if (path.EndsWith(": not found", StringComparison.Ordinal))
            {
                failures.Add(path);
                continue;
            }

            CheckFile(path, profile, resolver, failures);
        }

        return failures;
    }

    static IEnumerable<string> ExpandPaths(IEnumerable<string> rootPaths)
    {
        foreach (var path in rootPaths)
        {
            if (File.Exists(path) && path.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            {
                yield return path;
                continue;
            }

            if (Directory.Exists(path))
            {
                foreach (var file in Directory.EnumerateFiles(path, "*.md", SearchOption.AllDirectories))
                    yield return file;
                continue;
            }

            yield return $"{path}: not found";
        }
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
            if (!DocAnchorWire.LooksLikeDocEnvelope(envelope.Inner))
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

            if (!DocAnchorWire.HasDocFamily(wire))
                continue;

            if (!resolver.TryResolve(wire, out var resolveError))
                failures.Add($"{filePath}:{LineNumber(text, envelope.Start)}: {resolveError} wire={envelope.Wire}");
        }
    }

    static int LineNumber(string text, int index) => text.Take(index).Count(c => c == '\n') + 1;
}
