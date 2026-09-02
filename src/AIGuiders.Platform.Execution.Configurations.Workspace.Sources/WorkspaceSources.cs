#nullable enable



using AIGuiders.Platform.Combinations.Sources;

using AIGuiders.Platform.Combinations.Workspace;

using AIGuiders.Platform.Execution.Sources;



namespace AIGuiders.Platform.Execution.Configurations.Workspace;



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

        SourceCombination.Merge(

            baseline,

            overlay,

            WorkspaceCombinators.FieldOverlay,

            sourceId ?? $"{baseline.SourceId}+{overlay.SourceId}");

    public static WorkspaceExploreCorrPolicy.Mode ResolveExploreCorrMode(
        string absoluteFilePath,
        string workspaceRoot)
    {
        var doc = TryLoadCascade(workspaceRoot);
        return WorkspaceExploreCorrPolicy.ResolveMode(
            absoluteFilePath,
            workspaceRoot,
            doc?.Workspace?.ExploreCorr);
    }

}
