using AIGuiders.Platform.Authoring.Core;

namespace AIGuiders.Platform.Authoring.Command.Catalog.Parsing;

public sealed class CatalogParseContext
{
    public string? Planet { get; set; }
    public List<CatalogImport> Imports { get; } = [];
    public Dictionary<string, string> DefaultsKv { get; } = new(StringComparer.OrdinalIgnoreCase);
    public List<CatalogChannel> Channels { get; } = [];
    public List<CatalogVariable> Variables { get; } = [];
    public List<CatalogHelp> Helps { get; } = [];
    public List<CatalogPhrase> Phrases { get; } = [];
    public List<CatalogProfile> Profiles { get; } = [];
    public List<CatalogCommandRow> Commands { get; } = [];
    public List<CatalogBindingRow> Bindings { get; } = [];
    public List<CatalogMelodyRow> Melodies { get; } = [];
    public List<CatalogMcpRow> Mcp { get; } = [];
    public Dictionary<string, string> Executors { get; } = new(StringComparer.OrdinalIgnoreCase);
    public List<AuthoringDiagnostic> Diagnostics { get; } = [];

    public CatalogDocument BuildDocument() =>
        new()
        {
            Planet = Planet ?? "",
            Imports = Imports.Select(static i => i.Path).ToList(),
            Defaults = BuildDefaults(),
            Channels = Channels,
            Variables = Variables,
            Helps = Helps,
            Phrases = Phrases,
            Profiles = Profiles,
            Commands = Commands,
            Bindings = Bindings,
            Melodies = Melodies,
            Mcp = Mcp,
            Executors = Executors,
        };

    CatalogDefaults BuildDefaults() =>
        new()
        {
            VariableKind = DefaultsKv.GetValueOrDefault("variable.kind"),
            CommandScope = DefaultsKv.GetValueOrDefault("command.scope"),
            CommandSurfaces = KvSurface.ParseList(DefaultsKv.GetValueOrDefault("command.surfaces")),
            GrammarKeyboardBinding = DefaultsKv.GetValueOrDefault("grammar.keyboard.binding"),
            GrammarKeyboardMelody = DefaultsKv.GetValueOrDefault("grammar.keyboard.melody"),
            BindingChordRoot = DefaultsKv.GetValueOrDefault("binding.chord-root"),
        };

    public void ValidateChannels()
    {
        foreach (var channel in Channels)
        {
            if (string.IsNullOrWhiteSpace(channel.Surface))
            {
                continue;
            }

            if (channel.Surface.Equals("palette", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(channel.CommandGrammar) || string.IsNullOrWhiteSpace(channel.ArgumentGrammar))
            {
                Diagnostics.Add(new(
                    AuthoringDiagnosticCode.MissingGrammarDeclaration,
                    $"Channel `{channel.Surface}{(channel.Sub is null ? "" : "." + channel.Sub)}` missing `grammar` block with command and argument.",
                    1,
                    Section: "channels"));
            }
        }
    }
}
