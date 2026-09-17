#nullable disable

using AIGuiders.Platform.Modeling.Ide.Session;
using AIGuiders.Platform.Modeling.Language.Adapters.Fcs;
using FSharp.Compiler.CodeAnalysis;
using Microsoft.FSharp.Core;

namespace AIGuiders.Platform.Execution.Language.Adapters.Fcs;

/// <summary>
/// FCS host boundary (plan §7) — MSBuild/ProjInfo materialization + revision cache.
/// Modeling keeps language transforms; Execution owns host lifecycle until full File IO split.
/// </summary>
public static class FcsExecutionHost
{
    static FcsExecutionHost()
    {
        FcsProjectOptions.bindSource(new FcsExecutionProjectOptionsSource());
    }

    public static WorkspaceView Materialize(WorkspaceView view) =>
        FcsCompilerServicesHost.materialize(view);

    public static FSharpOption<FSharpProjectOptions> TryGetOptions(string projectPath) =>
        FcsCompilerServicesHost.tryGetOptions(projectPath);

    public static WorkspaceView TryGetView(string anchorPath)
    {
        var opt = FcsCompilerServicesHost.tryGetView(anchorPath);
        return FSharpOption<WorkspaceView>.get_IsSome(opt) ? opt.Value : null;
    }

    public static void Invalidate(string anchorPath) =>
        FcsCompilerServicesHost.invalidate(
            string.IsNullOrWhiteSpace(anchorPath)
                ? FSharpOption<string>.None
                : FSharpOption<string>.Some(anchorPath));
}
