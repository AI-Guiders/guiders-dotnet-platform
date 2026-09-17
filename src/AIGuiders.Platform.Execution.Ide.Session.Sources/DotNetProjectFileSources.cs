using System.Xml.Linq;

namespace AIGuiders.Platform.Execution.Ide.Session;

/// <summary>csproj/fsproj XML read — Execution IO (plan §7).</summary>
public static class DotNetProjectFileSources
{
    static readonly HashSet<string> IncludeLocalNames = new(StringComparer.Ordinal)
    {
        "Compile",
        "Content",
        "EmbeddedResource",
    };

    public static IReadOnlyList<string> ReadProjectReferences(string projectPath)
    {
        if (!File.Exists(projectPath))
            return [];

        var dir = Path.GetDirectoryName(projectPath) ?? string.Empty;
        var doc = XDocument.Load(projectPath);

        return doc.Descendants()
            .Where(el => el.Name.LocalName == "ProjectReference")
            .Select(el => el.Attribute("Include")?.Value)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => Path.GetFullPath(Path.Combine(dir, v!.Replace('/', Path.DirectorySeparatorChar))))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static IReadOnlyList<string> ReadSourceFiles(string projectPath)
    {
        if (!File.Exists(projectPath))
            return [];

        var dir = Path.GetDirectoryName(projectPath) ?? string.Empty;
        var doc = XDocument.Load(projectPath);

        var explicitSources = doc.Descendants()
            .Where(el => IncludeLocalNames.Contains(el.Name.LocalName))
            .Select(el => el.Attribute("Include")?.Value)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => Path.GetFullPath(Path.Combine(dir, v!.Replace('/', Path.DirectorySeparatorChar))))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (explicitSources.Count > 0)
            return explicitSources;

        return IsSdkStyle(doc) ? ReadSdkStyleSources(dir, projectPath) : [];
    }

    static bool IsSdkStyle(XDocument doc) =>
        doc.Root?.Attribute("Sdk") is { Value: { Length: > 0 } };

    static IReadOnlyList<string> ReadSdkStyleSources(string dir, string projectPath)
    {
        var patterns = Path.GetExtension(projectPath) switch
        {
            ".fsproj" => new[] { "*.fs" },
            ".csproj" => new[] { "*.cs" },
            _ => Array.Empty<string>(),
        };

        return patterns
            .SelectMany(pattern => Directory.GetFiles(dir, pattern, SearchOption.AllDirectories))
            .Select(Path.GetFullPath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
