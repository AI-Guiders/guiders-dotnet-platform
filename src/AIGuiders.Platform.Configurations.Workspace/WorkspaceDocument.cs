#nullable enable



namespace AIGuiders.Platform.Configurations.Workspace;



public sealed class WorkspaceDocument

{

    public WorkspaceSection? Workspace { get; set; }

}



public sealed class WorkspaceSection

{

    public WorkspaceAdrSettings? Adr { get; set; }

    public WorkspaceFeatures? Features { get; set; }

    public WorkspaceCorrespondenceSettings? Correspondence { get; set; }

    public WorkspaceExploreCorrSettings? ExploreCorr { get; set; }

}



public sealed class WorkspaceExploreCorrSettings

{

    public string? Default { get; set; }

    public List<WorkspaceExploreCorrRule> Rules { get; set; } = [];

}



public sealed class WorkspaceExploreCorrRule

{

    public string? Path { get; set; }

    public string? Mode { get; set; }

}



public sealed class WorkspaceAdrSettings

{

    public string? AutoInclude { get; set; }

    public int? MaxRelated { get; set; }

    public string? RootDir { get; set; }

    public Dictionary<string, object>? Map { get; set; }

}



public sealed class WorkspaceFeatures

{

    public List<WorkspaceFeature> Feature { get; set; } = [];

}



public sealed class WorkspaceFeature

{

    public string? Id { get; set; }

    public string? Title { get; set; }

    public List<string> Paths { get; set; } = [];

    public List<string> Docs { get; set; } = [];

}



public sealed class WorkspaceCorrespondenceSettings

{

    public List<WorkspaceCodeAnchor> CodeAnchors { get; set; } = [];

}



public sealed class WorkspaceCodeAnchor

{

    public string? Doc { get; set; }

    public string? File { get; set; }

    public string? Bracket { get; set; }

    public int? LineStart { get; set; }

    public int? LineEnd { get; set; }

    public string? Kind { get; set; }

    public string? MemberKey { get; set; }

}


