namespace AIGuiders.Platform.Authoring.Emit;

public sealed class GdlSatResult
{
    public bool Success { get; init; }

    public bool Supported { get; init; }

    public string? Summary { get; init; }

    public IReadOnlyList<GdlDiagnostic> Diagnostics { get; init; } = [];

    public static GdlSatResult NotSupported(string summary) =>
        new() { Success = true, Supported = false, Summary = summary };
}
