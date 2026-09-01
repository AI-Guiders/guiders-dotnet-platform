namespace AIGuiders.Platform.Authoring.Core;

public sealed record AuthoringImportDirective(
    AuthoringImportTargetKind TargetKind,
    string Path,
    string? Alias = null,
    bool LegacyIncludeKeyword = false);
