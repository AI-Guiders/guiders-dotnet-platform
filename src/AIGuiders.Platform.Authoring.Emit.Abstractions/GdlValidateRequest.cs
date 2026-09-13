namespace AIGuiders.Platform.Authoring.Emit;

public sealed class GdlValidateRequest
{
    public required string Path { get; init; }

    public required string Lang { get; init; }

    public string? Surface { get; init; }

    public string? WorkspaceRoot { get; init; }
}
