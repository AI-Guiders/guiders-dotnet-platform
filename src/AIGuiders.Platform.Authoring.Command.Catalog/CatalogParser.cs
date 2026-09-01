using System.Text;
using AIGuiders.Platform.Authoring.Core;

namespace AIGuiders.Platform.Authoring.Command.Catalog;

public static class CatalogParser
{
    public static CatalogParseResult Parse(string text, string? sourcePath = null) =>
        ParseLines(AuthoringSource.FromText(text), sourcePath);

    public static CatalogParseResult ParseFile(string path) =>
        ParseLines(AuthoringSource.FromFile(path), path);

    internal static CatalogParseResult ParseLines(IReadOnlyList<AuthoringLine> lines, string? sourcePath)
    {
        var diagnostics = new List<AuthoringDiagnostic>();
        string? planet = null;
        var imports = new List<string>();
        var defaults = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var channels = new List<CatalogChannel>();
        var variables = new List<CatalogVariable>();
        var helps = new List<CatalogHelp>();
        var phrases = new List<CatalogPhrase>();
        var commands = new List<CatalogCommandRow>();
        var bindings = new List<CatalogBindingRow>();
        var melodies = new List<CatalogMelodyRow>();
        var mcp = new List<CatalogMcpRow>();
        var executors = new Dictionary<string, string>(StringComparer.Ordinal);

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line.Text))
            {
                continue;
            }

            if (line.Text.StartsWith("catalog ", StringComparison.Ordinal))
            {
                planet = line.Text["catalog ".Length..].Trim();
                continue;
            }

            if (line.Text.StartsWith("import ", StringComparison.Ordinal))
            {
                imports.Add(line.Text["import ".Length..].Trim().Trim('<', '>'));
                continue;
            }

            if (!BlockReader.TryParseOpener(line.Text, out var opener))
            {
                continue;
            }

            var block = BlockReader.Read(lines, i + 1, opener.Keyword, diagnostics);
            i = block.EndLineIndex;
            if (!block.IsClosed)
            {
                continue;
            }

            var surfaceKind = BlockReader.ResolveSurfaceKind(opener);
            ApplyBlock(
                opener.Keyword,
                surfaceKind,
                block.Body,
                diagnostics,
                defaults,
                channels,
                variables,
                helps,
                phrases,
                commands,
                bindings,
                melodies,
                mcp,
                executors);
        }

        if (string.IsNullOrWhiteSpace(planet))
        {
            diagnostics.Add(new(AuthoringDiagnosticCode.MissingCatalogHeader, "Missing `catalog <planet>` header.", 1));
            return new() { Diagnostics = diagnostics };
        }

        var document = new CatalogDocument
        {
            Planet = planet,
            Imports = imports,
            Defaults = BuildDefaults(defaults),
            Channels = channels,
            Variables = variables,
            Helps = helps,
            Phrases = phrases,
            Commands = commands,
            Bindings = bindings,
            Melodies = melodies,
            Mcp = mcp,
            Executors = executors,
        };

        CatalogGrammarValidator.Validate(document, diagnostics);
        ValidateChannels(document, diagnostics);

        return new() { Document = document, Diagnostics = diagnostics };
    }

    static void ApplyBlock(
        string section,
        AuthoringSurfaceKind surfaceKind,
        IReadOnlyList<AuthoringLine> body,
        List<AuthoringDiagnostic> diagnostics,
        Dictionary<string, string> defaults,
        List<CatalogChannel> channels,
        List<CatalogVariable> variables,
        List<CatalogHelp> helps,
        List<CatalogPhrase> phrases,
        List<CatalogCommandRow> commands,
        List<CatalogBindingRow> bindings,
        List<CatalogMelodyRow> melodies,
        List<CatalogMcpRow> mcp,
        Dictionary<string, string> executors)
    {
        switch (section)
        {
            case "defaults":
                KvSurface.MergeInto(defaults, body);
                break;
            case "channels":
                channels.AddRange(ParseChannels(body));
                break;
            case "variables":
                if (surfaceKind == AuthoringSurfaceKind.Table)
                {
                    ParseVariablesTable(body, variables);
                }
                else
                {
                    ParseVariablesKv(body, variables, defaults);
                }

                break;
            case "helps":
                if (surfaceKind == AuthoringSurfaceKind.Table)
                {
                    ParseHelps(body, helps);
                }

                break;
            case "phrases":
                if (surfaceKind == AuthoringSurfaceKind.Table)
                {
                    ParsePhrases(body, phrases);
                }

                break;
            case "commands":
                ParseCommands(body, commands);
                break;
            case "bindings":
                ParseBindings(body, bindings);
                break;
            case "melodies":
                ParseMelodies(body, melodies);
                break;
            case "mcp":
                ParseMcp(body, mcp);
                break;
            case "executors":
                KvSurface.MergeInto(executors, body);
                break;
            default:
                diagnostics.Add(new(
                    AuthoringDiagnosticCode.UnknownSection,
                    $"Unknown section `{section}`.",
                    body.Count > 0 ? body[0].LineNumber : 1,
                    Section: section));
                break;
        }
    }

    static IReadOnlyList<CatalogChannel> ParseChannels(IReadOnlyList<AuthoringLine> body)
    {
        if (body.Count > 0 && body[0].Text.TrimStart().StartsWith('|'))
        {
            return ParseChannelsTable(body);
        }

        var filtered = InnerBlockFilter.StripEndMarkers(body);
        var treeLines = filtered.Select(static l => (l.LineNumber, l.Text));
        var list = new List<CatalogChannel>();
        foreach (var surfaceNode in IndentedTreeParser.Parse(treeLines))
        {
            var lineGrammar = ReadLineGrammar(surfaceNode);

            if (surfaceNode.Value is not null)
            {
                list.Add(new CatalogChannel
                {
                    Surface = surfaceNode.Key,
                    PlanetId = surfaceNode.Value,
                    CommandGrammar = lineGrammar.Command,
                    ArgumentGrammar = lineGrammar.Argument,
                });
                continue;
            }

            foreach (var child in surfaceNode.Children)
            {
                if (child.Key.Equals("grammar", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                list.Add(new CatalogChannel
                {
                    Surface = surfaceNode.Key,
                    Sub = child.Key,
                    PlanetId = child.Value,
                    CommandGrammar = lineGrammar.Command,
                    ArgumentGrammar = lineGrammar.Argument,
                });
            }
        }

        return list;
    }

    static (string? Command, string? Argument) ReadLineGrammar(IndentedNode surfaceNode)
    {
        var grammarNode = surfaceNode.Children.FirstOrDefault(static c =>
            c.Key.Equals("grammar", StringComparison.OrdinalIgnoreCase) && c.Value is null);

        if (grammarNode is null)
        {
            return (null, null);
        }

        string? command = null;
        string? argument = null;
        foreach (var child in grammarNode.Children)
        {
            if (child.Key.Equals("command", StringComparison.OrdinalIgnoreCase))
            {
                command = child.Value;
            }
            else if (child.Key.Equals("argument", StringComparison.OrdinalIgnoreCase))
            {
                argument = child.Value;
            }
        }

        return (command, argument);
    }

    static IReadOnlyList<CatalogChannel> ParseChannelsTable(IReadOnlyList<AuthoringLine> body)
    {
        var list = new List<CatalogChannel>();
        foreach (var map in TableSurface.ParseMaps(body))
        {
            list.Add(new CatalogChannel
            {
                Surface = map.GetValueOrDefault("surface") ?? "",
                Sub = TableSurface.NullIfEmpty(map.GetValueOrDefault("sub")),
                PlanetId = TableSurface.NullIfEmpty(map.GetValueOrDefault("planet-id")),
                CommandGrammar = TableSurface.NullIfEmpty(map.GetValueOrDefault("grammar.command")),
                ArgumentGrammar = TableSurface.NullIfEmpty(map.GetValueOrDefault("grammar.argument")),
            });
        }

        return list;
    }

    static void ParseVariablesKv(
        IReadOnlyList<AuthoringLine> body,
        List<CatalogVariable> variables,
        Dictionary<string, string> defaults)
    {
        foreach (var entry in KvSurface.ParseNameOrPair(body))
        {
            variables.Add(new(entry.Name, entry.Value ?? defaults.GetValueOrDefault("variable.kind")));
        }
    }

    static void ParseVariablesTable(IReadOnlyList<AuthoringLine> body, List<CatalogVariable> variables)
    {
        foreach (var map in TableSurface.ParseMaps(body))
        {
            variables.Add(new(map.GetValueOrDefault("name") ?? "", TableSurface.NullIfEmpty(map.GetValueOrDefault("kind"))));
        }
    }

    static void ParseHelps(IReadOnlyList<AuthoringLine> body, List<CatalogHelp> helps)
    {
        foreach (var map in TableSurface.ParseMaps(body))
        {
            helps.Add(new(
                map.GetValueOrDefault("target") ?? "",
                map.GetValueOrDefault("field") ?? "",
                map.GetValueOrDefault("text") ?? ""));
        }
    }

    static void ParsePhrases(IReadOnlyList<AuthoringLine> body, List<CatalogPhrase> phrases)
    {
        foreach (var map in TableSurface.ParseMaps(body))
        {
            phrases.Add(new(map.GetValueOrDefault("name") ?? "", map.GetValueOrDefault("phrase") ?? ""));
        }
    }

    static void ParseCommands(IReadOnlyList<AuthoringLine> body, List<CatalogCommandRow> commands)
    {
        foreach (var map in TableSurface.ParseMaps(body))
        {
            var command = map.GetValueOrDefault("command") ?? "";
            commands.Add(new CatalogCommandRow { Command = command, Columns = map });
        }
    }

    static void ParseBindings(IReadOnlyList<AuthoringLine> body, List<CatalogBindingRow> bindings)
    {
        foreach (var map in TableSurface.ParseMaps(body))
        {
            bindings.Add(new(
                map.GetValueOrDefault("gesture") ?? "",
                map.GetValueOrDefault("command") ?? "",
                TableSurface.NullIfEmpty(map.GetValueOrDefault("role"))));
        }
    }

    static void ParseMelodies(IReadOnlyList<AuthoringLine> body, List<CatalogMelodyRow> melodies)
    {
        foreach (var map in TableSurface.ParseMaps(body))
        {
            melodies.Add(new(map.GetValueOrDefault("slug") ?? "", map.GetValueOrDefault("command") ?? ""));
        }
    }

    static void ParseMcp(IReadOnlyList<AuthoringLine> body, List<CatalogMcpRow> mcp)
    {
        foreach (var map in TableSurface.ParseMaps(body))
        {
            mcp.Add(new(map.GetValueOrDefault("command") ?? "", map.GetValueOrDefault("expose") ?? "yes"));
        }
    }

    static CatalogDefaults BuildDefaults(Dictionary<string, string> kv) =>
        new()
        {
            VariableKind = kv.GetValueOrDefault("variable.kind"),
            CommandScope = kv.GetValueOrDefault("command.scope"),
            CommandSurfaces = KvSurface.ParseList(kv.GetValueOrDefault("command.surfaces")),
            GrammarKeyboardBinding = kv.GetValueOrDefault("grammar.keyboard.binding"),
            GrammarKeyboardMelody = kv.GetValueOrDefault("grammar.keyboard.melody"),
            BindingChordRoot = kv.GetValueOrDefault("binding.chord-root"),
        };

    static void ValidateChannels(CatalogDocument document, List<AuthoringDiagnostic> diagnostics)
    {
        foreach (var channel in document.Channels)
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
                diagnostics.Add(new(
                    AuthoringDiagnosticCode.MissingGrammarDeclaration,
                    $"Channel `{channel.Surface}{(channel.Sub is null ? "" : "." + channel.Sub)}` missing `grammar` block with command and argument.",
                    1,
                    Section: "channels"));
            }
        }
    }
}
