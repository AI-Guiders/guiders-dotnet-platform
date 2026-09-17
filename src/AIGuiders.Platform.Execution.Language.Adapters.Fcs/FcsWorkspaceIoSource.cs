#nullable disable

using System;
using System.IO;
using AIGuiders.Platform.Modeling.Core.Identity;
using AIGuiders.Platform.Modeling.Ide.Session;
using AIGuiders.Platform.Modeling.Ide.Session.Ports.DotNet;
using AIGuiders.Platform.Modeling.Language.Adapters.Fcs;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

namespace AIGuiders.Platform.Execution.Language.Adapters.Fcs;

/// <summary>
/// Disk-backed source text + slnx graph load for FCS backend (plan §7 — File IO @ Execution only).
/// </summary>
public sealed class FcsWorkspaceIoSource : IFcsSourceTextSource, IFcsSolutionGraphSource
{
    public FSharpOption<string> TryRead(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return FSharpOption<string>.None;

        try
        {
            return FSharpOption<string>.Some(File.ReadAllText(path));
        }
        catch
        {
            return FSharpOption<string>.None;
        }
    }

    public FSharpOption<SolutionGraph> TryLoadGraph(string anchorPath)
    {
        if (string.IsNullOrWhiteSpace(anchorPath) || !File.Exists(anchorPath))
            return FSharpOption<SolutionGraph>.None;

        try
        {
            return FSharpOption<SolutionGraph>.Some(DotNetSlnxGraphPort.load(anchorPath));
        }
        catch
        {
            return FSharpOption<SolutionGraph>.None;
        }
    }

    public FSharpOption<FSharpMap<string, ProjectId>> TryLoadOwnership(string anchorPath)
    {
        if (string.IsNullOrWhiteSpace(anchorPath) || !File.Exists(anchorPath))
            return FSharpOption<FSharpMap<string, ProjectId>>.None;

        try
        {
            var ownership = DotNetSlnxGraphPort.loadDocumentOwnership(anchorPath);
            return FSharpOption<FSharpMap<string, ProjectId>>.Some(ownership);
        }
        catch
        {
            return FSharpOption<FSharpMap<string, ProjectId>>.None;
        }
    }
}
