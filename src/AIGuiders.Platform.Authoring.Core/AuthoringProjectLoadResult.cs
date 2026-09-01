namespace AIGuiders.Platform.Authoring.Core;

public sealed class AuthoringProjectLoadResult
{
    public AuthoringProject? Project { get; init; }

    public IReadOnlyList<AuthoringDiagnostic> Diagnostics { get; init; } = [];
}
