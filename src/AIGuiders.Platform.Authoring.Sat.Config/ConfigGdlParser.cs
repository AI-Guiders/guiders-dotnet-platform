using AIGuiders.Platform.Authoring.Core;
using AIGuiders.Platform.Authoring.Emit;

namespace AIGuiders.Platform.Authoring.Sat;

public sealed class ConfigParseResult
{
    public ConfigDocument? Document { get; init; }

    public IReadOnlyList<GdlDiagnostic> Diagnostics { get; init; } = [];
}

public static class ConfigGdlParser
{
    public static ConfigParseResult ParseFile(string path)
    {
        var lines = AuthoringSource.FromFile(path);
        return Parse(lines, path);
    }

    public static ConfigParseResult ParseText(string text) =>
        Parse(AuthoringSource.FromText(text));

    public static ConfigParseResult Parse(IReadOnlyList<AuthoringLine> lines, string? sourcePath = null)
    {
        var diagnostics = new List<GdlDiagnostic>();
        string? name = null;
        string? basedOnAdr = null;
        var defaults = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var contracts = new List<ConfigContractRow>();

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            var text = line.Text.Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            if (text.StartsWith("config ", StringComparison.OrdinalIgnoreCase))
            {
                name = text["config ".Length..].Trim();
                continue;
            }

            if (text.StartsWith("based on adr:", StringComparison.OrdinalIgnoreCase))
            {
                basedOnAdr = text["based on adr:".Length..].Trim();
                continue;
            }

            if (text.StartsWith("import ", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (text.Equals("defaults", StringComparison.OrdinalIgnoreCase))
            {
                i = ParseDefaultsBlock(lines, i + 1, defaults, diagnostics);
                continue;
            }

            if (text.Equals("contracts table", StringComparison.OrdinalIgnoreCase))
            {
                i = ParseContractsTable(lines, i + 1, contracts, diagnostics);
                continue;
            }

            if (text.EndsWith(" table", StringComparison.OrdinalIgnoreCase))
            {
                i = SkipTableBody(lines, i + 1);
            }
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            diagnostics.Add(new GdlDiagnostic(
                "config-missing-header",
                "Missing `config <name>` header.",
                lines.Count > 0 ? lines[0].LineNumber : 1));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return new ConfigParseResult { Diagnostics = diagnostics };
        }

        return new ConfigParseResult
        {
            Document = new ConfigDocument
            {
                Name = name,
                BasedOnAdr = basedOnAdr,
                Defaults = defaults,
                Contracts = contracts,
            },
            Diagnostics = diagnostics,
        };
    }

    private static int ParseDefaultsBlock(
        IReadOnlyList<AuthoringLine> lines,
        int startIndex,
        IDictionary<string, string> defaults,
        IList<GdlDiagnostic> diagnostics)
    {
        for (var i = startIndex; i < lines.Count; i++)
        {
            var text = lines[i].Text.Trim();
            if (text.Equals("end defaults", StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            if (KvSurface.TryParsePair(lines[i].Text, out var key, out var value))
            {
                defaults[key] = value;
                continue;
            }

            diagnostics.Add(new GdlDiagnostic(
                "config-invalid-default",
                $"Invalid defaults entry `{text}`.",
                lines[i].LineNumber));
        }

        diagnostics.Add(new GdlDiagnostic(
            "config-unclosed-defaults",
            "Unclosed `defaults` block (expected `end defaults`).",
            startIndex > 0 ? lines[startIndex - 1].LineNumber : 1));
        return lines.Count - 1;
    }

    private static int ParseContractsTable(
        IReadOnlyList<AuthoringLine> lines,
        int startIndex,
        IList<ConfigContractRow> contracts,
        IList<GdlDiagnostic> diagnostics)
    {
        var tableLines = new List<AuthoringLine>();
        var i = startIndex;
        for (; i < lines.Count; i++)
        {
            var text = lines[i].Text.Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            if (!text.StartsWith('|'))
            {
                break;
            }

            tableLines.Add(lines[i]);
        }

        var maps = TableSurface.ParseMaps(tableLines);
        foreach (var map in maps)
        {
            if (!map.TryGetValue("id", out var id) || string.IsNullOrWhiteSpace(id))
            {
                diagnostics.Add(new GdlDiagnostic(
                    "config-contract-missing-id",
                    "Contracts table row is missing `id`.",
                    tableLines.Count > 0 ? tableLines[^1].LineNumber : startIndex + 1));
                continue;
            }

            contracts.Add(new ConfigContractRow(
                id,
                map.GetValueOrDefault("requires", string.Empty),
                map.GetValueOrDefault("ensures", string.Empty),
                tableLines.Count > 0 ? tableLines[0].LineNumber : startIndex + 1));
        }

        return Math.Max(startIndex, i - 1);
    }

    private static int SkipTableBody(IReadOnlyList<AuthoringLine> lines, int startIndex)
    {
        var i = startIndex;
        for (; i < lines.Count; i++)
        {
            var text = lines[i].Text.Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            if (!text.StartsWith('|'))
            {
                break;
            }
        }

        return Math.Max(startIndex, i - 1);
    }
}
