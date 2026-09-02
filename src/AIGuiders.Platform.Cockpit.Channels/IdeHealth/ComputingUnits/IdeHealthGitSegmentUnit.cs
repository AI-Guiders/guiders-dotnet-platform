#nullable enable

using AIGuiders.Platform.Cockpit.ComputingUnits;

namespace AIGuiders.Platform.Cockpit.Channels.IdeHealth.ComputingUnits;

public sealed class IdeHealthGitSegmentUnit : ICockpitComputeUnit
{
    public static IdeHealthGitSegmentUnit Default { get; } = new();

    public IdeHealthSegmentInput Compose(string gitLine, string gitCockpitShort) =>
        IdeHealthFormattingUnit.Default.GitSegment(gitLine, gitCockpitShort);
}
