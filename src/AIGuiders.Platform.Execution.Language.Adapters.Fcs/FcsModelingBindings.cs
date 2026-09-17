#nullable disable

using System.Threading;

namespace AIGuiders.Platform.Execution.Language.Adapters.Fcs;

/// <summary>
/// Binds all F# Modeling FCS ports when this assembly loads (plan §7).
/// </summary>
internal static class FcsModelingBindings
{
    static int initialized;

    internal static void EnsureInitialized()
    {
        if (Interlocked.CompareExchange(ref initialized, 1, 0) != 0)
            return;

        var workspaceIo = new FcsWorkspaceIoSource();
        AIGuiders.Platform.Modeling.Language.Adapters.Fcs.FcsProjectOptions.bindSource(new FcsExecutionProjectOptionsSource());
        AIGuiders.Platform.Modeling.Language.Adapters.Fcs.FcsSessionPatchApply.bindApplier(new FcsSessionPatchApplier());
        AIGuiders.Platform.Modeling.Language.Adapters.Fcs.FcsSourceText.bindSource(workspaceIo);
        AIGuiders.Platform.Modeling.Language.Adapters.Fcs.FcsSolutionGraph.bindSource(workspaceIo);
    }
}
