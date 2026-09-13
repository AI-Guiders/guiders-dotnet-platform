using AIGuiders.Platform.Authoring.Core;
using AIGuiders.Platform.Authoring.Emit;
using AIGuiders.Platform.Authoring.Sat;

namespace AIGuiders.Platform.Authoring.Sat.IdeSession;

public sealed class IdeSessionCatalogParseResult
{
    public IdeSessionGateCatalog? Catalog { get; init; }

    public IReadOnlyList<GdlDiagnostic> Diagnostics { get; init; } = [];
}

public static class IdeSessionCatalogParser
{
    const string GatesSectionKeyword = "gates";

    public static IdeSessionCatalogParseResult ParseFile(string path) =>
        ParseLines(AuthoringSource.FromFile(path), path);

    public static IdeSessionCatalogParseResult Parse(string text, string? sourcePath = null) =>
        ParseLines(AuthoringSource.FromText(text), sourcePath ?? "<text>");

    static IdeSessionCatalogParseResult ParseLines(IReadOnlyList<AuthoringLine> lines, string sourcePath)
    {
        var diagnostics = new List<GdlDiagnostic>();

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            if (!BlockReader.TryParseOpener(line.Text, out var opener)
                || !string.Equals(opener.Keyword, GatesSectionKeyword, StringComparison.OrdinalIgnoreCase)
                || opener.Kind != AuthoringSurfaceKind.Table)
            {
                continue;
            }

            var block = BlockReader.Read(lines, i + 1, GatesSectionKeyword);
            if (!block.IsClosed)
            {
                diagnostics.Add(new(
                    "ide-session.gates.unclosed",
                    "Unclosed `gates table` block.",
                    line.LineNumber));
                return new() { Diagnostics = diagnostics };
            }

            var gates = ParseGateRows(TableSurface.ParseMaps(block.Body), diagnostics, sourcePath);
            return new()
            {
                Catalog = new IdeSessionGateCatalog
                {
                    SourcePath = sourcePath,
                    Gates = gates,
                },
                Diagnostics = diagnostics,
            };
        }

        diagnostics.Add(new(
            "ide-session.gates.missing",
            "Missing `gates table` block.",
            1));
        return new() { Diagnostics = diagnostics };
    }

    static IReadOnlyList<IdeSessionGateRow> ParseGateRows(
        IReadOnlyList<Dictionary<string, string>> rows,
        IList<GdlDiagnostic> diagnostics,
        string sourcePath)
    {
        var gates = new List<IdeSessionGateRow>();
        foreach (var row in rows)
        {
            if (!row.TryGetValue("gate", out var gateId) || string.IsNullOrWhiteSpace(gateId))
            {
                diagnostics.Add(new(
                    "ide-session.gates.row",
                    "Gate row is missing the `gate` column.",
                    1));
                continue;
            }

            row.TryGetValue("reject-when", out var rejectWhen);
            row.TryGetValue("code", out var code);
            gates.Add(new IdeSessionGateRow(
                gateId.Trim(),
                (rejectWhen ?? string.Empty).Trim(),
                (code ?? string.Empty).Trim()));
        }

        if (gates.Count == 0)
        {
            diagnostics.Add(new(
                "ide-session.gates.empty",
                $"No gate rows found in `{sourcePath}`.",
                1));
        }

        return gates;
    }
}
