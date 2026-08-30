#nullable enable

using AIGuiders.Platform.Configurations.Workspace;

namespace AIGuiders.Platform.Combinations.Workspace;

public static class WorkspaceDocumentOverlays
{
    /// <summary>Overlay merge via <see cref="WorkspaceOverlay.FieldOverlay"/> policy (ADR-0031).</summary>
    public static WorkspaceDocument MergeOver(this WorkspaceDocument baseline, WorkspaceDocument overlay) =>
        WorkspaceCombinators.FieldOverlay(baseline, overlay);
}
