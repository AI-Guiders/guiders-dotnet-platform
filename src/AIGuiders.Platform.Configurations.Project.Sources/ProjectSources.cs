#nullable enable

using AIGuiders.Platform.Sources;

namespace AIGuiders.Platform.Configurations.Project;

public static class ProjectSources
{
    public const string CdpRelativeFile = ".cdp/project.toml";
    public const string AltRelativeFile = "cdp-project.toml";

    public static IFormatReader<ProjectDocument> TomlReader { get; } = TomlFormatReader<ProjectDocument>.SnakeCase;

    public static string ResolveFile(string workRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workRoot);
        var primary = Path.Combine(workRoot, CdpRelativeFile.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(primary))
            return primary;
        var alt = Path.Combine(workRoot, AltRelativeFile);
        if (File.Exists(alt))
            return alt;
        return primary;
    }

    public static ISource<ProjectDocument> FromText(string text, string? sourceId = null) =>
        SourceCatalog.FromText(text, TomlReader, sourceId);

    public static ISource<ProjectDocument> FromFile(string path, string? sourceId = null) =>
        FileSources.FromFile(path, TomlReader, sourceId);

    public static ProjectDocument? TryLoadFile(string workRoot)
    {
        var path = ResolveFile(workRoot);
        if (!File.Exists(path))
            return null;
        try
        {
            return FromFile(path).Load();
        }
        catch
        {
            return null;
        }
    }

    public static ProjectDocument MergeDocuments(ProjectDocument baseline, ProjectDocument? overlay) =>
        overlay is null ? baseline : overlay.MergeOver(baseline);
}
