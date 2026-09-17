using AIGuiders.Platform.Execution.Documentation.Correspondence;

namespace AIGuiders.Platform.Authoring.Sat;

/// <summary>Deprecated — use <see cref="GoldenEvidence"/> in Execution.Documentation.Correspondence.Core.</summary>
[Obsolete("Use AIGuiders.Platform.Execution.Documentation.Correspondence.GoldenEvidence.")]
public static class GoldenEvidenceBridge
{
    public static bool Exists(string workspaceRoot, string goldenId) =>
        GoldenEvidence.Exists(workspaceRoot, goldenId);
}
