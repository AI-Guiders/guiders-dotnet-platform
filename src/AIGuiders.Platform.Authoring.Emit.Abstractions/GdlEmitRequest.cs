namespace AIGuiders.Platform.Authoring.Emit;

public sealed class GdlEmitRequest
{
    public required string Path { get; init; }

    public required string Lang { get; init; }

    public string? Surface { get; init; }

    public string? WorkspaceRoot { get; init; }

    public required string Namespace { get; init; }

    public required string ClassName { get; init; }

    public string? OutputPath { get; init; }
}
