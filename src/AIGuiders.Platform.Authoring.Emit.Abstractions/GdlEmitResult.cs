namespace AIGuiders.Platform.Authoring.Emit;

public sealed class GdlEmitResult
{
    public bool Success { get; init; }

    public string? GeneratedCode { get; init; }

    public IReadOnlyList<GdlDiagnostic> Diagnostics { get; init; } = [];
}
