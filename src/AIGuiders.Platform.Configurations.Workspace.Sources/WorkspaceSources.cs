#nullable enable

using AIGuiders.Platform.Sources;

namespace AIGuiders.Platform.Configurations.Workspace;

public static class WorkspaceSources
{
    public const string CascadeRelativePath = ".cascade/workspace.toml";

    public static IFormatReader<WorkspaceDocument> TomlReader { get; } = TomlFormatReader<WorkspaceDocument>.SnakeCase;

    public static string CascadeTomlPath(string workspaceRoot) =>
        Path.Combine(workspaceRoot, CascadeRelativePath.Replace('/', Path.DirectorySeparatorChar));

    public static ISource<WorkspaceDocument> FromText(string text, string? sourceId = null) =>
        SourceCatalog.FromText(text, TomlReader, sourceId);

    public static ISource<WorkspaceDocument> FromFile(string path, string? sourceId = null) =>
        FileSources.FromFile(path, TomlReader, sourceId);

    public static ISource<WorkspaceDocument> FromCascadeRoot(string workspaceRoot, string? sourceId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workspaceRoot);
        return FromFile(CascadeTomlPath(workspaceRoot), sourceId ?? "workspace:cascade");
    }

    public static WorkspaceDocument? TryLoadCascade(string workspaceRoot)
    {
        var path = CascadeTomlPath(workspaceRoot);
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

    /// <summary>Overlay merge: baseline (e.g. embedded defaults) + overlay (e.g. disk).</summary>
    public static ISource<WorkspaceDocument> MergeOverlay(
        ISource<WorkspaceDocument> baseline,
        ISource<WorkspaceDocument> overlay,
        string? sourceId = null) =>
        SourceCatalog.Merge(
            baseline,
            overlay,
            static (b, o) => b.MergeOver(o),
            sourceId ?? $"{baseline.SourceId}+{overlay.SourceId}");
}
