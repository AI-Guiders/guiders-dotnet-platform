namespace AIGuiders.Platform.Authoring.Sat;

public sealed class IdeSessionGateCatalog
{
    public required string SourcePath { get; init; }

    public IReadOnlyList<IdeSessionGateRow> Gates { get; init; } = [];
}

public sealed record IdeSessionGateRow(string GateId, string RejectWhen, string Code);
