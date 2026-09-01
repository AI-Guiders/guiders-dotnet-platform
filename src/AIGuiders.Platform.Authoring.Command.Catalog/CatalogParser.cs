using System.Text;
using AIGuiders.Platform.Authoring.Core;

namespace AIGuiders.Platform.Authoring.Command.Catalog;

public static class CatalogParser
{
    public static CatalogParseResult Parse(string text, string? sourcePath = null) =>
        ParseLines(text.Replace("\r\n", "\n").Split('\n'), sourcePath);

    public static CatalogParseResult ParseFile(string path)
    {
        var text = File.ReadAllText(path, Encoding.UTF8);
        return Parse(text, path);
    }

    internal static CatalogParseResult ParseLines(IReadOnlyList<string> rawLines, string? sourcePath)
    {
        var diagnostics = new List<AuthoringDiagnostic>();
        var lines = rawLines
            .Select((t, i) => (Line: i + 1, Text: StripComment(t)))
            .ToList();

        string? planet = null;
        var imports = new List<string>();
        var defaults = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var channels = new List<CatalogChannel>();
        var variables = new List<CatalogVariable>();
        var helps = new List<CatalogHelp>();
        var phrases = new List<CatalogPhrase>();
        var profiles = new List<CatalogProfile>();
        var commands = new List<CatalogCommandRow>();
        var bindings = new List<CatalogBindingRow>();
        var melodies = new List<CatalogMelodyRow>();
        var mcp = new List<CatalogMcpRow>();
        var executors = new Dictionary<string, string>(StringComparer.Ordinal);

        for (var i = 0; i < lines.Count; i++)
        {
            var (lineNo, line) = lines[i];
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (line.StartsWith("catalog ", StringComparison.Ordinal))
            {
                planet = line["catalog ".Length..].Trim();
                continue;
            }

            if (line.StartsWith("import ", StringComparison.Ordinal))
            {
                imports.Add(line["import ".Length..].Trim().Trim('<', '>'));
                continue;
            }

            if (TryOpenBlock(line, out var section, out var isTable))
            {
                i = ParseBlock(
                    lines,
                    i + 1,
                    section,
                    isTable,
                    diagnostics,
                    defaults,
                    channels,
                    variables,
                    helps,
                    phrases,
                    profiles,
                    commands,
                    bindings,
                    melodies,
                    mcp,
                    executors);
            }
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
            Profiles = profiles,
            Commands = commands,
            Bindings = bindings,
            Melodies = melodies,
            Mcp = mcp,
            Executors = executors,
        };

        CatalogNotationValidator.Validate(document, diagnostics);
        ValidateChannels(document, diagnostics);

        return new() { Document = document, Diagnostics = diagnostics };
    }

    static int ParseBlock(
        IReadOnlyList<(int Line, string Text)> lines,
        int start,
        string section,
        bool isTable,
        List<AuthoringDiagnostic> diagnostics,
        Dictionary<string, string> defaults,
        List<CatalogChannel> channels,
        List<CatalogVariable> variables,
        List<CatalogHelp> helps,
        List<CatalogPhrase> phrases,
        List<CatalogProfile> profiles,
        List<CatalogCommandRow> commands,
        List<CatalogBindingRow> bindings,
        List<CatalogMelodyRow> melodies,
        List<CatalogMcpRow> mcp,
        Dictionary<string, string> executors)
    {
        var body = new List<(int Line, string Text)>();
        var i = start;
        for (; i < lines.Count; i++)
        {
            var (lineNo, text) = lines[i];
            if (text.StartsWith($"end {section}", StringComparison.Ordinal))
            {
                break;
            }

            if (!string.IsNullOrWhiteSpace(text))
            {
                body.Add((lineNo, text));
            }
        }

        if (i >= lines.Count)
        {
            diagnostics.Add(new(AuthoringDiagnosticCode.InvalidSyntax, $"Unclosed block `{section}`.", start));
            return i;
        }

        switch (section)
        {
            case "defaults":
                ParseDefaultsKv(body, defaults);
                break;
            case "channels":
                channels.AddRange(ParseChannels(body));
                break;
            case "variables":
                if (isTable)
                {
                    ParseVariablesTable(body, variables, diagnostics);
                }
                else
                {
                    ParseVariablesKv(body, variables, defaults);
                }

                break;
            case "helps":
                ParseHelps(body, isTable, helps, diagnostics);
                break;
            case "phrases":
                ParsePhrases(body, isTable, phrases, diagnostics);
                break;
            case "commands":
                ParseCommands(body, commands, diagnostics);
                break;
            case "bindings":
                ParseBindings(body, bindings, diagnostics);
                break;
            case "melodies":
                ParseMelodies(body, melodies, diagnostics);
                break;
            case "mcp":
                ParseMcp(body, mcp, diagnostics);
                break;
            case "executors":
                ParseExecutorsKv(body, executors);
                break;
            default:
                diagnostics.Add(new(AuthoringDiagnosticCode.UnknownSection, $"Unknown section `{section}`.", start, Section: section));
                break;
        }

        return i;
    }

    static bool TryOpenBlock(string line, out string section, out bool isTable)
    {
        section = "";
        isTable = false;
        var trimmed = line.Trim();
        if (trimmed.EndsWith(" table", StringComparison.Ordinal))
        {
            isTable = true;
            section = trimmed[..^" table".Length].Trim();
            return true;
        }

        if (trimmed is "defaults" or "channels" or "variables" or "helps" or "phrases" or "profiles"
            or "commands" or "bindings" or "melodies" or "mcp" or "executors")
        {
            section = trimmed;
            return true;
        }

        return false;
    }

    static void ParseDefaultsKv(IReadOnlyList<(int Line, string Text)> body, Dictionary<string, string> defaults)
    {
        foreach (var (_, text) in body)
        {
            var eq = text.IndexOf('=');
            if (eq <= 0)
            {
                continue;
            }

            defaults[text[..eq].Trim()] = text[(eq + 1)..].Trim();
        }
    }

    static IReadOnlyList<CatalogChannel> ParseChannels(IReadOnlyList<(int Line, string Text)> body)
    {
        if (body.Count > 0 && body[0].Text.TrimStart().StartsWith('|'))
        {
            return ParseChannelsTable(body);
        }

        var list = new List<CatalogChannel>();
        foreach (var surfaceNode in IndentedTreeParser.Parse(body))
        {
            var surfaceNotations = ReadNotation(surfaceNode);

            if (surfaceNode.Value is not null)
            {
                list.Add(new CatalogChannel
                {
                    Surface = surfaceNode.Key,
                    PlanetId = surfaceNode.Value,
                    CommandNotation = surfaceNotations.Command,
                    ArgumentNotation = surfaceNotations.Argument,
                });
                continue;
            }

            foreach (var child in surfaceNode.Children)
            {
                if (IsNotationKey(child.Key))
                {
                    continue;
                }

                list.Add(new CatalogChannel
                {
                    Surface = surfaceNode.Key,
                    Sub = child.Key,
                    PlanetId = child.Value,
                    CommandNotation = surfaceNotations.Command,
                    ArgumentNotation = surfaceNotations.Argument,
                });
            }
        }

        return list;
    }

    static bool IsNotationKey(string key) =>
        key.Equals("command-notation", StringComparison.OrdinalIgnoreCase)
        || key.Equals("argument-notation", StringComparison.OrdinalIgnoreCase);

    static (string? Command, string? Argument) ReadNotation(IndentedNode node)
    {
        string? command = null;
        string? argument = null;
        foreach (var child in node.Children)
        {
            if (child.Key.Equals("command-notation", StringComparison.OrdinalIgnoreCase))
            {
                command = child.Value;
            }
            else if (child.Key.Equals("argument-notation", StringComparison.OrdinalIgnoreCase))
            {
                argument = child.Value;
            }
        }

        return (command, argument);
    }

    static IReadOnlyList<CatalogChannel> ParseChannelsTable(IReadOnlyList<(int Line, string Text)> body)
    {
        var rows = ParseTable(body);
        if (rows.Count == 0)
        {
            return [];
        }

        var header = rows[0];
        var data = rows.Skip(1);
        var list = new List<CatalogChannel>();
        foreach (var row in data)
        {
            var map = RowToMap(header, row);
            list.Add(new CatalogChannel
            {
                Surface = map.GetValueOrDefault("surface") ?? "",
                Sub = NullIfEmpty(map.GetValueOrDefault("sub")),
                PlanetId = NullIfEmpty(map.GetValueOrDefault("planet-id")),
                CommandNotation = NullIfEmpty(map.GetValueOrDefault("command-notation")),
                ArgumentNotation = NullIfEmpty(map.GetValueOrDefault("argument-notation")),
            });
        }

        return list;
    }

    static void ParseVariablesKv(
        IReadOnlyList<(int Line, string Text)> body,
        List<CatalogVariable> variables,
        Dictionary<string, string> defaults)
    {
        foreach (var (_, text) in body)
        {
            var eq = text.IndexOf('=');
            if (eq > 0)
            {
                variables.Add(new(text[..eq].Trim(), text[(eq + 1)..].Trim()));
            }
            else
            {
                variables.Add(new(text.Trim(), defaults.GetValueOrDefault("variable.kind")));
            }
        }
    }

    static void ParseVariablesTable(
        IReadOnlyList<(int Line, string Text)> body,
        List<CatalogVariable> variables,
        List<AuthoringDiagnostic> diagnostics)
    {
        var rows = ParseTable(body);
        if (rows.Count == 0)
        {
            return;
        }

        var header = rows[0];
        foreach (var row in rows.Skip(1))
        {
            var map = RowToMap(header, row);
            variables.Add(new(map.GetValueOrDefault("name") ?? "", NullIfEmpty(map.GetValueOrDefault("kind"))));
        }
    }

    static void ParseHelps(
        IReadOnlyList<(int Line, string Text)> body,
        bool isTable,
        List<CatalogHelp> helps,
        List<AuthoringDiagnostic> diagnostics)
    {
        if (!isTable)
        {
            return;
        }

        var rows = ParseTable(body);
        if (rows.Count == 0)
        {
            return;
        }

        var header = rows[0];
        foreach (var row in rows.Skip(1))
        {
            var map = RowToMap(header, row);
            helps.Add(new(map.GetValueOrDefault("target") ?? "", map.GetValueOrDefault("field") ?? "", map.GetValueOrDefault("text") ?? ""));
        }
    }

    static void ParsePhrases(
        IReadOnlyList<(int Line, string Text)> body,
        bool isTable,
        List<CatalogPhrase> phrases,
        List<AuthoringDiagnostic> diagnostics)
    {
        if (!isTable)
        {
            return;
        }

        var rows = ParseTable(body);
        if (rows.Count == 0)
        {
            return;
        }

        var header = rows[0];
        foreach (var row in rows.Skip(1))
        {
            var map = RowToMap(header, row);
            phrases.Add(new(map.GetValueOrDefault("name") ?? "", map.GetValueOrDefault("phrase") ?? ""));
        }
    }

    static void ParseCommands(
        IReadOnlyList<(int Line, string Text)> body,
        List<CatalogCommandRow> commands,
        List<AuthoringDiagnostic> diagnostics)
    {
        var rows = ParseTable(body);
        if (rows.Count == 0)
        {
            return;
        }

        var header = rows[0];
        foreach (var row in rows.Skip(1))
        {
            var map = RowToMap(header, row);
            var command = map.GetValueOrDefault("command") ?? "";
            commands.Add(new CatalogCommandRow { Command = command, Columns = map });
        }
    }

    static void ParseBindings(
        IReadOnlyList<(int Line, string Text)> body,
        List<CatalogBindingRow> bindings,
        List<AuthoringDiagnostic> diagnostics)
    {
        var rows = ParseTable(body);
        if (rows.Count == 0)
        {
            return;
        }

        var header = rows[0];
        foreach (var row in rows.Skip(1))
        {
            var map = RowToMap(header, row);
            bindings.Add(new(
                map.GetValueOrDefault("gesture") ?? "",
                map.GetValueOrDefault("command") ?? "",
                NullIfEmpty(map.GetValueOrDefault("role"))));
        }
    }

    static void ParseMelodies(
        IReadOnlyList<(int Line, string Text)> body,
        List<CatalogMelodyRow> melodies,
        List<AuthoringDiagnostic> diagnostics)
    {
        var rows = ParseTable(body);
        if (rows.Count == 0)
        {
            return;
        }

        var header = rows[0];
        foreach (var row in rows.Skip(1))
        {
            var map = RowToMap(header, row);
            melodies.Add(new(map.GetValueOrDefault("slug") ?? "", map.GetValueOrDefault("command") ?? ""));
        }
    }

    static void ParseMcp(
        IReadOnlyList<(int Line, string Text)> body,
        List<CatalogMcpRow> mcp,
        List<AuthoringDiagnostic> diagnostics)
    {
        var rows = ParseTable(body);
        if (rows.Count == 0)
        {
            return;
        }

        var header = rows[0];
        foreach (var row in rows.Skip(1))
        {
            var map = RowToMap(header, row);
            mcp.Add(new(map.GetValueOrDefault("command") ?? "", map.GetValueOrDefault("expose") ?? "yes"));
        }
    }

    static void ParseExecutorsKv(
        IReadOnlyList<(int Line, string Text)> body,
        Dictionary<string, string> executors)
    {
        foreach (var (_, text) in body)
        {
            var eq = text.IndexOf('=');
            if (eq > 0)
            {
                executors[text[..eq].Trim()] = text[(eq + 1)..].Trim();
            }
        }
    }

    static List<IReadOnlyList<string>> ParseTable(IReadOnlyList<(int Line, string Text)> body)
    {
        var rows = new List<IReadOnlyList<string>>();
        foreach (var (_, text) in body)
        {
            if (!TableRowParser.TryParseRow(text, out var cells))
            {
                continue;
            }

            if (TableRowParser.IsSeparatorRow(cells))
            {
                continue;
            }

            rows.Add(cells);
        }

        return rows;
    }

    static Dictionary<string, string> RowToMap(IReadOnlyList<string> header, IReadOnlyList<string> row)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < header.Count && i < row.Count; i++)
        {
            map[header[i]] = row[i];
        }

        return map;
    }

    static CatalogDefaults BuildDefaults(Dictionary<string, string> kv) =>
        new()
        {
            VariableKind = kv.GetValueOrDefault("variable.kind"),
            CommandScope = kv.GetValueOrDefault("command.scope"),
            CommandSurfaces = SplitList(kv.GetValueOrDefault("command.surfaces")),
            NotationKeyboardBinding = kv.GetValueOrDefault("notation.keyboard.binding"),
            NotationKeyboardMelody = kv.GetValueOrDefault("notation.keyboard.melody"),
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

            if (string.IsNullOrWhiteSpace(channel.CommandNotation) || string.IsNullOrWhiteSpace(channel.ArgumentNotation))
            {
                diagnostics.Add(new(
                    AuthoringDiagnosticCode.MissingNotationDeclaration,
                    $"Channel `{channel.Surface}{(channel.Sub is null ? "" : "." + channel.Sub)}` missing command-notation or argument-notation.",
                    1,
                    Section: "channels"));
            }
        }
    }

    static IReadOnlyList<string> SplitList(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

    static string StripComment(string line)
    {
        var hash = line.IndexOf('#');
        return hash >= 0 ? line[..hash].TrimEnd() : line;
    }

    static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;
}
