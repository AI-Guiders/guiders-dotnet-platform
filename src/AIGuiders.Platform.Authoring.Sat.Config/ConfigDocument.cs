namespace AIGuiders.Platform.Authoring.Sat;

/// <summary>
/// Pilot IR for <c>*.config.gdl</c> (GUIDERS-ADR-0064 v0): header, defaults, contracts.
/// </summary>
public sealed class ConfigDocument
{
    public required string Name { get; init; }

    public string? BasedOnAdr { get; init; }

    public IReadOnlyDictionary<string, string> Defaults { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<ConfigSourceRow> Sources { get; init; } = [];

    public IReadOnlyList<ConfigContractRow> Contracts { get; init; } = [];

    public IReadOnlyList<ConfigFactRow> Facts { get; init; } = [];
}

public sealed record ConfigSourceRow(
    string Id,
    string Kind,
    string Path,
    string Slice,
    int Line = 1);

public sealed record ConfigContractRow(
    string Id,
    string Requires,
    string Ensures,
    int Line = 1);

public sealed record ConfigFactRow(
    string Contract,
    string VerifiedBy,
    int Line = 1);
