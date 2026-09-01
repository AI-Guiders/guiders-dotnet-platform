using System.Reflection;
using AIGuiders.Platform.Authoring.Command.Catalog;
using AIGuiders.Platform.Authoring.Command.Catalog.Parsing;
using AIGuiders.Platform.Authoring.Command.Catalog.Parsing.Sections;
using AIGuiders.Platform.Authoring.Core;

namespace AIGuiders.Platform.Authoring.Command.Bundles;

public static class CatalogBundleParser
{
    public static IReadOnlyList<CatalogProfile> ParseProfiles(string text)
    {
        var lines = AuthoringSource.FromText(text);
        if (lines.Count == 0 || !BlockReader.TryParseOpener(lines[0].Text, out var opener))
        {
            return [];
        }

        var block = BlockReader.Read(lines, 1, opener.Keyword);
        var context = new CatalogParseContext();
        new ProfilesSectionHandler().Apply(context, new AuthoringSectionBlock(
            opener.Keyword,
            opener.Kind,
            block.Body));
        return context.Profiles;
    }
}

public sealed class CatalogBundleLibrary : ICatalogBundleLibrary
{
    public static CatalogBundleLibrary Federation { get; } = new();

    static readonly IReadOnlyDictionary<string, string> Embedded = LoadEmbedded();

    public bool TryResolve(string importPath, out IReadOnlyList<CatalogProfile> profiles)
    {
        profiles = [];
        if (!Embedded.TryGetValue(NormalizePath(importPath), out var text))
        {
            return false;
        }

        profiles = CatalogBundleParser.ParseProfiles(text);
        return profiles.Count > 0;
    }

    static string NormalizePath(string path) =>
        path.Replace('\\', '/').Trim('/');

    static IReadOnlyDictionary<string, string> LoadEmbedded()
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var assembly = typeof(CatalogBundleLibrary).Assembly;
        foreach (var name in assembly.GetManifestResourceNames())
        {
            if (!name.EndsWith(".catalogbundle", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            using var stream = assembly.GetManifestResourceStream(name);
            if (stream is null)
            {
                continue;
            }

            using var reader = new StreamReader(stream);
            var text = reader.ReadToEnd();
            var marker = ".Bundles.";
            var start = name.LastIndexOf(marker, StringComparison.Ordinal);
            var relative = start >= 0
                ? name[(start + marker.Length)..].Replace(".catalogbundle", "", StringComparison.OrdinalIgnoreCase)
                : name;
            if (relative.StartsWith("Bundles.", StringComparison.Ordinal))
            {
                relative = relative["Bundles.".Length..];
            }

            map[NormalizePath(relative.Replace('.', '/'))] = text;
        }

        return map;
    }
}
