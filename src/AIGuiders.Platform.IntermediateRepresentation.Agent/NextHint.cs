namespace AIGuiders.Platform.IntermediateRepresentation.Agent;

public sealed record NextHint(
    string Kind,
    string? CommandId = null,
    string? ToolName = null,
    string? Label = null);
