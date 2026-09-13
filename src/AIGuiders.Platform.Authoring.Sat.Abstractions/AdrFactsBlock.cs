namespace AIGuiders.Platform.Authoring.Sat;

public sealed class AdrFactsBlock
{
    public required string SourcePath { get; init; }

    public string? AdrId { get; init; }

    public IReadOnlyList<string> GoldenIds { get; init; } = [];

    public IReadOnlyList<HoareObligation> HoareObligations { get; init; } = [];

    public IReadOnlyList<string> WellFormednessIds { get; init; } = [];

    public IReadOnlyList<AdrVerifiedByRow> VerifiedBy { get; init; } = [];
}

public sealed record AdrVerifiedByRow(string Subject, string Evidence);
