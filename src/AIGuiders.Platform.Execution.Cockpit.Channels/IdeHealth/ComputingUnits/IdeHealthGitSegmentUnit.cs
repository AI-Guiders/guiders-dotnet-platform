#nullable enable

using AIGuiders.Platform.Execution.Cockpit.ComputingUnits;

namespace AIGuiders.Platform.Execution.Cockpit.Channels.IdeHealth.ComputingUnits;

public sealed class IdeHealthGitSegmentUnit : ICockpitComputeUnit
{
    public static IdeHealthGitSegmentUnit Default { get; } = new();

    public IdeHealthSegmentInput Compose(string gitLine, string gitCockpitShort) =>
        IdeHealthFormattingUnit.Default.GitSegment(gitLine, gitCockpitShort);
}
