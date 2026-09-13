namespace AIGuiders.Platform.Authoring.Emit;

public interface IGdlQuarryPlugin
{
    string QuarryId { get; }

    IReadOnlyList<string> SupportedLanguages { get; }

    string? SurfaceId { get; }

    bool CanHandle(string quarryId, string lang, string? surface);

    GdlEmitResult Emit(GdlEmitRequest request);

    GdlValidateResult Validate(GdlValidateRequest request);

    GdlSatResult Sat(GdlSatRequest request);
}
