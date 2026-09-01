namespace AIGuiders.Platform.Authoring.Command.Catalog;

public sealed class CatalogDocument
{
    public required string Planet { get; init; }
    public IReadOnlyList<string> Imports { get; init; } = [];
    public CatalogDefaults Defaults { get; init; } = new();
    public IReadOnlyList<CatalogChannel> Channels { get; init; } = [];
    public IReadOnlyList<CatalogVariable> Variables { get; init; } = [];
    public IReadOnlyList<CatalogHelp> Helps { get; init; } = [];
    public IReadOnlyList<CatalogPhrase> Phrases { get; init; } = [];
    public IReadOnlyList<CatalogProfile> Profiles { get; init; } = [];
    public IReadOnlyList<CatalogCommandRow> Commands { get; init; } = [];
    public IReadOnlyList<CatalogBindingRow> Bindings { get; init; } = [];
    public IReadOnlyList<CatalogMelodyRow> Melodies { get; init; } = [];
    public IReadOnlyList<CatalogMcpRow> Mcp { get; init; } = [];
    public IReadOnlyDictionary<string, string> Executors { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);
}

public sealed class CatalogDefaults
{
    public string? VariableKind { get; init; }
    public string? CommandScope { get; init; }
    public IReadOnlyList<string> CommandSurfaces { get; init; } = [];
    public string? GrammarKeyboardBinding { get; init; }
    public string? GrammarKeyboardMelody { get; init; }
    public string? BindingChordRoot { get; init; }
}

public sealed class CatalogChannel
{
    public required string Surface { get; init; }
    public string? Sub { get; init; }
    public string? PlanetId { get; init; }
    public string? CommandGrammar { get; init; }
    public string? ArgumentGrammar { get; init; }
}

public sealed record CatalogVariable(string Name, string? Kind);
public sealed record CatalogHelp(string Target, string Field, string Text);
public sealed record CatalogPhrase(string Name, string Phrase);

public sealed class CatalogProfile
{
    public required string Name { get; init; }
    public IReadOnlyDictionary<string, string> Fields { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}

public sealed class CatalogCommandRow
{
    public required string Command { get; init; }
    public IReadOnlyDictionary<string, string> Columns { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}

public sealed record CatalogBindingRow(string Gesture, string Command, string? Role);
public sealed record CatalogMelodyRow(string Slug, string Command);
public sealed record CatalogMcpRow(string Command, string Expose);
