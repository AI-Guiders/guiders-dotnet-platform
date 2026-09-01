namespace AIGuiders.Platform.Authoring.Core;

public enum AuthoringImportTargetKind
{
    /// <summary>Quoted path — repo-relative logical file or glob (GUIDERS-ADR-0052).</summary>
    LogicalPath,

    /// <summary>Angle-bracket wire — federation bundle or planet stdlib, not filesystem.</summary>
    WireLibrary,
}
