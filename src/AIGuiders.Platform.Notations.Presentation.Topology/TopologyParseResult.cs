using AIGuiders.Platform.IntermediateRepresentation.Presentation;

namespace AIGuiders.Platform.Notations.Presentation.Topology;

public sealed class TopologyParseResult
{
    public PresentationTopology? Topology { get; init; }
    public string? Error { get; init; }
    public bool IsSuccess => Topology is not null && Error is null;

    public static TopologyParseResult Ok(PresentationTopology topology) =>
        new() { Topology = topology };

    public static TopologyParseResult Fail(string error) =>
        new() { Error = error };
}
