using GdlCorrespondence = AIGuiders.Platform.Modeling.Documentation.Correspondence;

namespace AIGuiders.Platform.Execution.Documentation.Correspondence;

/// <summary>Workspace golden-session evidence scan — F# SSOT in Modeling.Documentation.Correspondence (ship-3).</summary>
public static class GoldenEvidence
{
    public static bool Exists(string workspaceRoot, string goldenId) =>
        GdlCorrespondence.GoldenEvidence.exists(workspaceRoot, goldenId);
}
