#nullable enable

using AIGuiders.Platform.Configurations.Workspace;

namespace AIGuiders.Platform.Combinations.Workspace;

public static class WorkspaceCombinators
{
    public static CombinationSemantics Semantics => CombinationSemantics.FieldOverlay;

    public static Combinator<WorkspaceDocument> FieldOverlay { get; } = static (baseline, overlay) =>
        baseline.MergeOver(overlay);
}
