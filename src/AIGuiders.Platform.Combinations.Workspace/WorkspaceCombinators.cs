#nullable enable

using AIGuiders.Platform.Combinations.Overlay;
using AIGuiders.Platform.Configurations.Workspace;

namespace AIGuiders.Platform.Combinations.Workspace;

public static class WorkspaceCombinators
{
    public static CombinationSemantics Semantics => WorkspaceOverlay.FieldOverlay.Semantics;

    public static Combinator<WorkspaceDocument> FieldOverlay { get; } =
        WorkspaceOverlay.FieldOverlay.Combinator;
}
