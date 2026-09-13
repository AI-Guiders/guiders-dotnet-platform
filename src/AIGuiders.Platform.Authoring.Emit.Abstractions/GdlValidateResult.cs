namespace AIGuiders.Platform.Authoring.Emit;

public sealed class GdlValidateResult
{
    public bool Success { get; init; }

    public string? Summary { get; init; }

    public IReadOnlyList<GdlDiagnostic> Diagnostics { get; init; } = [];
}
