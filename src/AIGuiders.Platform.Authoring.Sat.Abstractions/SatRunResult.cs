using AIGuiders.Platform.Authoring.Emit;

namespace AIGuiders.Platform.Authoring.Sat;

public sealed class SatRunResult
{
    public bool Success { get; init; }

    public bool Supported { get; init; }

    public string? Summary { get; init; }

    public IReadOnlyList<GdlDiagnostic> Diagnostics { get; init; } = [];

    public static SatRunResult Ok(string summary) =>
        new() { Success = true, Supported = true, Summary = summary };

    public static SatRunResult Skipped(string summary) =>
        new() { Success = true, Supported = false, Summary = summary };

    public static SatRunResult Failed(IEnumerable<GdlDiagnostic> diagnostics, string? summary = null) =>
        new()
        {
            Success = false,
            Supported = true,
            Summary = summary,
            Diagnostics = diagnostics.ToArray(),
        };

    public GdlSatResult ToGdlSatResult() =>
        new()
        {
            Success = Success,
            Supported = Supported,
            Summary = Summary,
            Diagnostics = Diagnostics,
        };
}
