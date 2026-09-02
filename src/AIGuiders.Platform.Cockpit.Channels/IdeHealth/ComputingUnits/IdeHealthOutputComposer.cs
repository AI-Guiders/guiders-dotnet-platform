#nullable enable

namespace AIGuiders.Platform.Cockpit.Channels.IdeHealth.ComputingUnits;

public static class IdeHealthOutputComposer
{
    public static IdeHealthOutputSnapshot Compose(IdeHealthInputSnapshot input)
    {
        var segments = new[]
        {
            ToSegment(IdeHealthSource.Build, input.Solution.Build),
            ToSegment(IdeHealthSource.Tests, input.Solution.Tests),
            ToSegment(IdeHealthSource.Debug, input.Solution.Debug),
            ToSegment(IdeHealthSource.Git, input.Workspace.Git),
        };
        return new IdeHealthOutputSnapshot { Segments = segments, IdeHost = input.IdeHost };
    }

    static IdeHealthSegment ToSegment(IdeHealthSource source, IdeHealthSegmentInput seg) =>
        new()
        {
            Source = source,
            Stratum = seg.Stratum,
            Scope = seg.Scope,
            ProjectPath = seg.ProjectPath,
            LineText = seg.LineText,
            CockpitShort = seg.CockpitShort,
            IsBuildRunning = seg.IsBuildRunning,
        };
}
