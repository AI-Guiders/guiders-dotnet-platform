using AIGuiders.Platform.Authoring.Core;
using AIGuiders.Platform.IntermediateRepresentation.Presentation;
using AIGuiders.Platform.Notations.Presentation.Topology;

namespace AIGuiders.Platform.Authoring.Deck;

public static class DeckParser
{
    public static DeckParseResult Parse(string text, string? sourcePath = null) =>
        ParseLines(AuthoringSource.FromText(text), sourcePath);

    public static DeckParseResult ParseFile(string path) =>
        ParseLines(AuthoringSource.FromFile(path), path);

    static DeckParseResult ParseLines(IReadOnlyList<AuthoringLine> lines, string? sourcePath)
    {
        var diagnostics = new List<AuthoringDiagnostic>();
        string? planet = null;
        var presets = new List<AttentionPreset>();
        var zoneBindings = new Dictionary<string, string>(StringComparer.Ordinal);

        var i = 0;
        while (i < lines.Count)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line.Text))
            {
                i++;
                continue;
            }

            if (planet is null && line.Text.TrimStart().StartsWith("deck ", StringComparison.Ordinal))
            {
                planet = line.Text.Trim()["deck ".Length..].Trim();
                i++;
                continue;
            }

            if (line.Text.TrimStart().StartsWith("preset ", StringComparison.Ordinal))
            {
                var presetName = line.Text.Trim()["preset ".Length..].Trim();
                var block = BlockReader.Read(lines, i + 1, "preset", diagnostics);
                presets.Add(ParsePreset(presetName, block.Body, diagnostics));
                i = block.IsClosed ? block.EndLineIndex + 1 : lines.Count;
                continue;
            }

            if (line.Text.Trim().Equals("zones", StringComparison.OrdinalIgnoreCase))
            {
                var block = BlockReader.Read(lines, i + 1, "zones", diagnostics);
                MergeZoneBindings(block.Body, zoneBindings, diagnostics);
                i = block.IsClosed ? block.EndLineIndex + 1 : lines.Count;
                continue;
            }

            if (line.Text.TrimStart().StartsWith("end deck", StringComparison.Ordinal))
            {
                i++;
                continue;
            }

            diagnostics.Add(new(
                AuthoringDiagnosticCode.InvalidSyntax,
                $"Unexpected line in deck document: `{line.Text}`.",
                line.LineNumber));
            i++;
        }

        if (string.IsNullOrWhiteSpace(planet))
        {
            diagnostics.Add(new(
                AuthoringDiagnosticCode.MissingDeckHeader,
                "Missing `deck <planet>` header.",
                1));
            return new() { Diagnostics = diagnostics };
        }

        return new()
        {
            Document = new DeckDocument
            {
                Planet = planet,
                Presets = presets,
                ZoneBindings = zoneBindings,
            },
            Diagnostics = diagnostics,
        };
    }

    static AttentionPreset ParsePreset(
        string name,
        IReadOnlyList<AuthoringLine> body,
        IList<AuthoringDiagnostic> diagnostics)
    {
        string? forward = null;
        var mfdZones = new List<string>();
        string? eicas = null;
        PresentationTopology? topology = null;

        foreach (var line in body)
        {
            var text = line.Text.Trim();
            if (text.StartsWith("topology ", StringComparison.Ordinal))
            {
                var wire = text["topology ".Length..].Trim();
                var parsed = TopologyNotation.Parse(wire);
                if (!parsed.IsSuccess)
                {
                    diagnostics.Add(new(
                        AuthoringDiagnosticCode.TopologyWireInvalid,
                        parsed.Error ?? "Invalid topology wire.",
                        line.LineNumber));
                }
                else
                {
                    topology = parsed.Topology;
                }

                continue;
            }

            if (text.StartsWith("forward ", StringComparison.Ordinal))
            {
                forward = text["forward ".Length..].Trim();
                continue;
            }

            if (text.StartsWith("mfd ", StringComparison.Ordinal))
            {
                var tail = text["mfd ".Length..];
                foreach (var zone in tail.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                {
                    mfdZones.Add(zone);
                }

                continue;
            }

            if (text.StartsWith("eicas ", StringComparison.Ordinal))
            {
                eicas = text["eicas ".Length..].Trim();
                continue;
            }

            diagnostics.Add(new(
                AuthoringDiagnosticCode.InvalidSyntax,
                $"Unknown preset line: `{line.Text}`.",
                line.LineNumber));
        }

        return new AttentionPreset
        {
            Name = name,
            Topology = topology,
            ForwardZoneId = forward,
            MfdZoneIds = mfdZones,
            EicasPolicy = eicas,
        };
    }

    static void MergeZoneBindings(
        IReadOnlyList<AuthoringLine> body,
        IDictionary<string, string> zoneBindings,
        IList<AuthoringDiagnostic> diagnostics)
    {
        foreach (var line in body)
        {
            var parts = line.Text.Trim().Split('=', 2, StringSplitOptions.TrimEntries);
            if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]))
            {
                diagnostics.Add(new(
                    AuthoringDiagnosticCode.InvalidSyntax,
                    $"Expected `zone-id = role` in zones block: `{line.Text}`.",
                    line.LineNumber));
                continue;
            }

            if (zoneBindings.ContainsKey(parts[0]))
            {
                diagnostics.Add(new(
                    AuthoringDiagnosticCode.DuplicateRow,
                    $"Duplicate zone binding `{parts[0]}`.",
                    line.LineNumber));
            }

            zoneBindings[parts[0]] = parts[1];
        }
    }
}
