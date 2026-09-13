namespace AIGuiders.Platform.Authoring.Sat;

public sealed class SatContext
{
    public string? Path { get; init; }

    public string? ProjectPath { get; init; }

    public string? WorkspaceRoot { get; init; }

    public string Lang { get; init; } = "cs";

    public string? Surface { get; init; }

    public string? AdrId { get; init; }

    public string? FactsPath { get; init; }

    public AdrFactsBlock? ParsedFacts { get; init; }

    public IdeSessionGateCatalog? ParsedGateCatalog { get; init; }
}
