#nullable disable

using System.Collections.Concurrent;
using AIGuiders.Platform.Modeling.Ide.Session;
using AIGuiders.Platform.Modeling.Language.Adapters.Fcs;
using AIGuiders.Platform.Modeling.Paths;
using FSharp.Compiler.CodeAnalysis;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

namespace AIGuiders.Platform.Execution.Language.Adapters.Fcs;

/// <summary>
/// Materialize F# CompilerServices from <see cref="WorkspaceView"/> @ revision — MSBuild/ProjInfo once, then frozen.
/// Execution owns File/MSBuild IO (plan §7); Modeling keeps language transforms only.
/// </summary>
internal static class FcsCompilerServicesHost
{
    static readonly IFcsProjectOptionsSource MaterializeLoader = new FcsProbeProjectOptionsSource();
    static readonly ConcurrentDictionary<string, WorkspaceView> Views = new(StringComparer.OrdinalIgnoreCase);
    static readonly ConcurrentDictionary<string, FSharpProjectOptions> OptionsByProject =
        new(StringComparer.OrdinalIgnoreCase);

    public static FSharpOption<WorkspaceView> TryGetView(string anchorPath)
    {
        var key = AnchorKey(anchorPath);
        return Views.TryGetValue(key, out var view)
            ? FSharpOption<WorkspaceView>.Some(view)
            : FSharpOption<WorkspaceView>.None;
    }

    public static FSharpOption<FSharpProjectOptions> TryGetOptions(string projectPath)
    {
        var key = NormalizeProject(projectPath);
        if (string.IsNullOrWhiteSpace(key))
            return FSharpOption<FSharpProjectOptions>.None;

        return OptionsByProject.TryGetValue(key, out var options)
            ? FSharpOption<FSharpProjectOptions>.Some(options)
            : FSharpOption<FSharpProjectOptions>.None;
    }

    public static WorkspaceView Materialize(WorkspaceView view)
    {
        var key = view.Anchor.IsEmpty ? "" : view.Anchor.Value;
        var materialized = new List<WorkspaceProjectView>();
        foreach (var project in view.Projects)
            materialized.Add(MaterializeProject(project));

        var projects = ListModule.OfSeq(materialized);
        var enriched = new WorkspaceView(
            view.Revision,
            view.Anchor,
            view.Mode,
            view.RootProjectId,
            projects,
            view.Documents);
        Views[AnchorKey(key)] = enriched;
        return enriched;
    }

    public static void Invalidate(FSharpOption<string> anchorPath)
    {
        if (FSharpOption<string>.get_IsNone(anchorPath))
        {
            Views.Clear();
            OptionsByProject.Clear();
            return;
        }

        var path = anchorPath.Value;
        if (string.IsNullOrWhiteSpace(path))
            return;

        var key = AnchorKey(path);
        if (!Views.TryRemove(key, out var removed))
            return;

        foreach (var project in removed.Projects)
        {
            var projectKey = NormalizeProject(project.ProjectPath);
            if (!string.IsNullOrWhiteSpace(projectKey))
                OptionsByProject.TryRemove(projectKey, out _);
        }
    }

    static WorkspaceProjectView MaterializeProject(WorkspaceProjectView project)
    {
        if (!string.Equals(project.LanguageId, "fsharp", StringComparison.OrdinalIgnoreCase))
            return project;

        var load = MaterializeLoader.TryLoad(project.ProjectPath);
        if (load.IsOk)
        {
            var options = load.ResultValue;
            var key = NormalizeProject(project.ProjectPath);
            OptionsByProject[key] = options;
            return new WorkspaceProjectView(
                project.ProjectId,
                project.ProjectPath,
                project.LanguageId,
                ListModule.OfArray(options.SourceFiles));
        }

        throw new InvalidOperationException(
            $"F# compiler services materialize failed for '{project.ProjectPath}': {load.ErrorValue.Message}");
    }

    static string AnchorKey(string path) =>
        string.IsNullOrWhiteSpace(path)
            ? ""
            : LogicalPath.Create(Path.GetFullPath(path.Trim())).Value;

    static string NormalizeProject(string projectPath) =>
        string.IsNullOrWhiteSpace(projectPath)
            ? ""
            : Path.GetFullPath(projectPath.Trim());
}
