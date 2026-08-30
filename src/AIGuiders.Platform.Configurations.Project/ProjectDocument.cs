#nullable enable

namespace AIGuiders.Platform.Configurations.Project;

public sealed class ProjectDocument
{
    public ProjectTestSettings? Test { get; set; }
    public ProjectDocsSettings? Docs { get; set; }
    public ProjectFormatSettings? Format { get; set; }
    public ProjectCanonSettings? Canon { get; set; }

    public ProjectDocument MergeOver(ProjectDocument defaults) =>
        new()
        {
            Test = MergeTest(defaults.Test, Test),
            Docs = MergeDocs(defaults.Docs, Docs),
            Format = MergeFormat(defaults.Format, Format),
            Canon = MergeCanon(defaults.Canon, Canon),
        };

    static ProjectTestSettings? MergeTest(ProjectTestSettings? d, ProjectTestSettings? o) =>
        o is null ? d : new ProjectTestSettings
        {
            Framework = o.Framework ?? d?.Framework,
            Policy = o.Policy ?? d?.Policy,
        };

    static ProjectDocsSettings? MergeDocs(ProjectDocsSettings? d, ProjectDocsSettings? o) =>
        o is null ? d : new ProjectDocsSettings { Style = o.Style ?? d?.Style };

    static ProjectFormatSettings? MergeFormat(ProjectFormatSettings? d, ProjectFormatSettings? o) =>
        o is null ? d : new ProjectFormatSettings { Profile = o.Profile ?? d?.Profile };

    static ProjectCanonSettings? MergeCanon(ProjectCanonSettings? d, ProjectCanonSettings? o)
    {
        if (o is null)
            return d;
        if (d is null)
            return o;
        return new ProjectCanonSettings
        {
            Lang = o.Lang ?? d.Lang,
            OrgStyle = o.OrgStyle ?? d.OrgStyle,
            OrgStyleRoot = o.OrgStyleRoot ?? d.OrgStyleRoot,
            CanonFile = o.CanonFile ?? d.CanonFile,
            PreviewLines = o.PreviewLines ?? d.PreviewLines,
            BudgetPersonal = o.BudgetPersonal ?? d.BudgetPersonal,
            BudgetOrgCore = o.BudgetOrgCore ?? d.BudgetOrgCore,
            BudgetOrgLang = o.BudgetOrgLang ?? d.BudgetOrgLang,
            BudgetOrgLangDesign = o.BudgetOrgLangDesign ?? d.BudgetOrgLangDesign,
            BudgetProject = o.BudgetProject ?? d.BudgetProject,
            OperatorPrefsRelpath = o.OperatorPrefsRelpath ?? d.OperatorPrefsRelpath,
            OrgCoreFile = o.OrgCoreFile ?? d.OrgCoreFile,
            OrgLangFile = o.OrgLangFile ?? d.OrgLangFile,
            OrgLangDesignFile = o.OrgLangDesignFile ?? d.OrgLangDesignFile,
        };
    }
}

public sealed class ProjectTestSettings
{
    public string? Framework { get; set; }
    public string? Policy { get; set; }
}

public sealed class ProjectDocsSettings
{
    public string? Style { get; set; }
}

public sealed class ProjectFormatSettings
{
    public string? Profile { get; set; }
}

public sealed class ProjectCanonSettings
{
    public string? Lang { get; set; }
    public string? OrgStyle { get; set; }
    public string? OrgStyleRoot { get; set; }
    public string? CanonFile { get; set; }
    public int? PreviewLines { get; set; }
    public int? BudgetPersonal { get; set; }
    public int? BudgetOrgCore { get; set; }
    public int? BudgetOrgLang { get; set; }
    public int? BudgetOrgLangDesign { get; set; }
    public int? BudgetProject { get; set; }
    public string? OperatorPrefsRelpath { get; set; }
    public string? OrgCoreFile { get; set; }
    public string? OrgLangFile { get; set; }
    public string? OrgLangDesignFile { get; set; }
}
