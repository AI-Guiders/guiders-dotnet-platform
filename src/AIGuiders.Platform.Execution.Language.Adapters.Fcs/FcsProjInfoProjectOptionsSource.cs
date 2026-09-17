#nullable disable

using System.IO;
using AIGuiders.Platform.Modeling.Language.Adapters.Fcs;
using DotNetWorkspace.Core;
using Ionide.ProjInfo;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using FSharpProjectOptions = FSharp.Compiler.CodeAnalysis.FSharpProjectOptions;

namespace AIGuiders.Platform.Execution.Language.Adapters.Fcs;

/// <summary>
/// MSBuild design-time F# project options (Ionide.ProjInfo — plan §7 File IO @ Execution).
/// </summary>
public sealed class FcsProjInfoProjectOptionsSource : IFcsProjectOptionsSource
{
    public FSharpResult<FSharpProjectOptions, FcsProjectOptionsLoadError> TryLoad(string projectPath)
    {
        try
        {
            return LoadOptions(projectPath);
        }
        catch (Exception ex)
        {
            return FSharpResult<FSharpProjectOptions, FcsProjectOptionsLoadError>.NewError(
                new FcsProjectOptionsLoadError { Message = ex.Message });
        }
    }

    public void Warm(string projectPath)
    {
        try
        {
            LoadOptions(projectPath);
        }
        catch
        {
            // best-effort warm
        }
    }

    public void Invalidate(FSharpOption<string> fsprojPath = default) { }

    static FSharpResult<FSharpProjectOptions, FcsProjectOptionsLoadError> LoadOptions(string projectPath)
    {
        MsBuildLocatorOnce.EnsureRegistered();
        var full = Path.GetFullPath(projectPath);
        var projectDir = new DirectoryInfo(Path.GetDirectoryName(full)!);
        var toolsPath = Init.init(projectDir, FSharpOption<FileInfo>.None);
        var loader = WorkspaceLoader.Create(toolsPath, FSharpOption<FSharpList<Tuple<string, string>>>.None);

        var projectOptions = loader.LoadProjects(ListModule.OfArray(new[] { full })).FirstOrDefault();
        if (projectOptions is null)
        {
            return FSharpResult<FSharpProjectOptions, FcsProjectOptionsLoadError>.NewError(
                new FcsProjectOptionsLoadError { Message = $"Ionide.ProjInfo could not load '{full}'." });
        }

        var mapped = Ionide.ProjInfo.FCS.mapManyOptions(ListModule.OfArray(new[] { projectOptions })).FirstOrDefault();
        if (mapped is null)
        {
            return FSharpResult<FSharpProjectOptions, FcsProjectOptionsLoadError>.NewError(
                new FcsProjectOptionsLoadError { Message = $"Ionide.ProjInfo could not map FCS options for '{full}'." });
        }

        return FcsProjectOptionsGuards.requireFrameworkReferences(mapped);
    }
}

/// <summary>In-process ProjInfo load for tests/diagnostics.</summary>
public static class FcsProjInfoProjectOptions
{
    static readonly IFcsProjectOptionsSource Loader = new FcsProjInfoProjectOptionsSource();

    public static bool TryGet(string fsprojPath, out FSharpProjectOptions options)
    {
        options = null;
        if (string.IsNullOrWhiteSpace(fsprojPath) || !File.Exists(fsprojPath))
            return false;

        var result = Loader.TryLoad(fsprojPath);
        if (!result.IsOk)
            return false;

        options = result.ResultValue;
        return true;
    }
}
