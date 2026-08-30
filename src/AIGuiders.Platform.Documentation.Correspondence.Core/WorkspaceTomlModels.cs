#nullable enable

namespace AIGuiders.Platform.Documentation.Correspondence;

public sealed class WorkspaceTomlDoc
{
    public WorkspaceSection? Workspace { get; set; }
}

public sealed class WorkspaceSection
{
    public AdrToml? Adr { get; set; }
    public FeaturesToml? Features { get; set; }
    public CorrespondenceToml? Correspondence { get; set; }
}

public sealed class AdrToml
{
    public string? AutoInclude { get; set; }
    public int? MaxRelated { get; set; }
    public string? RootDir { get; set; }
    public Dictionary<string, object>? Map { get; set; }
}

public sealed class FeaturesToml
{
    public List<FeatureToml> Feature { get; set; } = [];
}

public sealed class FeatureToml
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public List<string> Paths { get; set; } = [];
    public List<string> Docs { get; set; } = [];
}

public sealed class CorrespondenceToml
{
    public List<CodeAnchorToml> CodeAnchors { get; set; } = [];
}

public sealed class CodeAnchorToml
{
    public string? Doc { get; set; }
    public string? File { get; set; }
    public string? Bracket { get; set; }
    public int? LineStart { get; set; }
    public int? LineEnd { get; set; }
    public string? Kind { get; set; }
    public string? MemberKey { get; set; }
}
