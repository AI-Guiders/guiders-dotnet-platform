namespace AIGuiders.Platform.MCPlane;

public sealed record NextHint(
    string Kind,
    string? CommandId = null,
    string? ToolName = null,
    string? Label = null);
