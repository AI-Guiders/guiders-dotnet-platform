using GdlCorrespondence = AIGuiders.Platform.Modeling.Documentation.Correspondence;

namespace AIGuiders.Platform.Authoring.Sat;

/// <summary>
/// Thin C# bridge to F# SSOT evidence locator in <c>Modeling.Gdl.Correspondence</c> (SAT-003).
/// </summary>
public static class GoldenEvidence
{
    public static bool Exists(string workspaceRoot, string goldenId) =>
        GdlCorrespondence.GoldenEvidence.exists(workspaceRoot, goldenId);
}
