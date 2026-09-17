#nullable disable

using System;
using System.IO;
using AIGuiders.Platform.Modeling.Core.Identity;
using AIGuiders.Platform.Modeling.Ide.Session;
using AIGuiders.Platform.Modeling.Language.Adapters.Fcs;
using DotNetWorkspace.Core;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

namespace AIGuiders.Platform.Execution.Language.Adapters.Fcs;

/// <summary>
/// Disk-backed fsproj ownership resolution for FCS backend (plan §7 — File/Directory IO @ Execution only).
/// </summary>
public sealed class FcsProjectOwnershipSource : IFcsProjectOwnershipSource
{
    readonly IFcsSolutionGraphSource _graphs;

    public FcsProjectOwnershipSource(IFcsSolutionGraphSource graphs) => _graphs = graphs;

    public FSharpOption<string> ResolveFsproj(string filePath, string solutionOrProjectPath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return FSharpOption<string>.None;

        if (string.IsNullOrWhiteSpace(solutionOrProjectPath) && !HasDirectoryComponent(filePath))
            return FSharpOption<string>.None;

        var walkUp = TryWalkUpToFsproj(filePath);
        if (FSharpOption<string>.get_IsSome(walkUp))
            return walkUp;

        if (!string.IsNullOrWhiteSpace(solutionOrProjectPath))
        {
            var fromGraph = TryResolveFromGraph(filePath, solutionOrProjectPath);
            if (FSharpOption<string>.get_IsSome(fromGraph))
                return fromGraph;
        }

        var hint = string.IsNullOrWhiteSpace(solutionOrProjectPath) ? null : solutionOrProjectPath;
        var entry = DotNetWorkspace.Core.DotNetWorkspace.TryResolveOwningProject(filePath, hint, DotNetProjectKind.FSharp);
        return entry is null
            ? FSharpOption<string>.None
            : FSharpOption<string>.Some(entry.AbsolutePath);
    }

    static bool HasDirectoryComponent(string filePath) =>
        !string.IsNullOrWhiteSpace(filePath)
        && !string.IsNullOrWhiteSpace(Path.GetDirectoryName(filePath));

    static string NormalizePath(string path) => Path.GetFullPath(path);

    static FSharpOption<string> TryWalkUpToFsproj(string filePath)
    {
        var full = NormalizePath(filePath);
        var startDir = Path.GetDirectoryName(full);
        if (string.IsNullOrWhiteSpace(startDir))
            startDir = full;

        return Walk(startDir, full);
    }

    static FSharpOption<string> Walk(string dir, string fullFilePath)
    {
        if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
            return FSharpOption<string>.None;

        string[] fsprojs;
        try
        {
            fsprojs = Directory.GetFiles(dir, "*.fsproj");
        }
        catch
        {
            return FSharpOption<string>.None;
        }

        switch (fsprojs.Length)
        {
            case 1:
                return FSharpOption<string>.Some(fsprojs[0]);
            case > 1:
            {
                var fileNameNoExt = Path.GetFileNameWithoutExtension(fullFilePath);
                foreach (var fsproj in fsprojs)
                {
                    var projName = Path.GetFileNameWithoutExtension(fsproj);
                    if (string.Equals(projName, fileNameNoExt, StringComparison.OrdinalIgnoreCase))
                        return FSharpOption<string>.Some(fsproj);
                }

                break;
            }
        }

        var parent = Directory.GetParent(dir);
        return parent is null
            ? FSharpOption<string>.None
            : Walk(parent.FullName, fullFilePath);
    }

    FSharpOption<string> TryResolveFromGraph(string filePath, string anchorPath)
    {
        var graphOpt = _graphs.TryLoadGraph(anchorPath);
        var ownershipOpt = _graphs.TryLoadOwnership(anchorPath);
        if (!FSharpOption<SolutionGraph>.get_IsSome(graphOpt)
            || !FSharpOption<FSharpMap<string, ProjectId>>.get_IsSome(ownershipOpt))
            return FSharpOption<string>.None;

        return TryOwnerProjectPath(ownershipOpt.Value, graphOpt.Value, filePath);
    }

    static FSharpOption<string> TryOwnerProjectPath(
        FSharpMap<string, ProjectId> ownership,
        SolutionGraph graph,
        string filePath)
    {
        var full = NormalizePath(filePath);
        FSharpOption<ProjectId> ownerId;

        if (ownership.ContainsKey(full))
            ownerId = FSharpOption<ProjectId>.Some(ownership[full]);
        else
        {
            ownerId = FSharpOption<ProjectId>.None;
            foreach (var pair in ownership)
            {
                if (string.Equals(NormalizePath(pair.Key), full, StringComparison.OrdinalIgnoreCase))
                {
                    ownerId = FSharpOption<ProjectId>.Some(pair.Value);
                    break;
                }
            }
        }

        if (!FSharpOption<ProjectId>.get_IsSome(ownerId))
            return FSharpOption<string>.None;

        ProjectNode project = null;
        foreach (var candidate in graph.Projects)
        {
            if (candidate.Id.Equals(ownerId.Value))
            {
                project = candidate;
                break;
            }
        }

        return project is null
            ? FSharpOption<string>.None
            : FSharpOption<string>.Some(project.AbsolutePath);
    }
}
