using AIGuiders.Platform.Authoring.Core;
using AIGuiders.Platform.Authoring.Emit;
using GdlAuthoring = AIGuiders.Platform.Modeling.Gdl.Authoring;
using GdlConfig = AIGuiders.Platform.Modeling.Gdl.Parse.Config;
using GdlConfigModel = AIGuiders.Platform.Modeling.Configurations;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

namespace AIGuiders.Platform.Authoring.Sat;

public sealed class ConfigParseResult
{
    public ConfigDocument? Document { get; init; }

    public IReadOnlyList<GdlDiagnostic> Diagnostics { get; init; } = [];
}

/// <summary>
/// Thin C# bridge to F# SSOT parser in <c>Modeling.Gdl.Parse.Config</c> (SAT-002).
/// </summary>
public static class ConfigGdlParser
{
    public static ConfigParseResult ParseFile(string path)
    {
        var text = File.ReadAllText(path);
        return ParseText(text);
    }

    public static ConfigParseResult ParseText(string text)
    {
        var result = GdlConfig.ConfigParser.parseText(text);
        return Map(result);
    }

    public static ConfigParseResult Parse(IReadOnlyList<AuthoringLine> lines, string? sourcePath = null)
    {
        _ = sourcePath;
        var mapped = ListModule.OfSeq(lines.Select(MapLine));
        var result = GdlConfig.ConfigParser.parse(mapped);
        return Map(result);
    }

    private static GdlAuthoring.AuthoringLine MapLine(AuthoringLine line) =>
        new GdlAuthoring.AuthoringLine(line.LineNumber, line.Text);

    private static ConfigParseResult Map(GdlConfig.ConfigParseResult result) =>
        new()
        {
            Document = result.Document is not null && FSharpOption<GdlConfigModel.ConfigDocument>.get_IsSome(result.Document)
                ? MapDocument(result.Document.Value)
                : null,
            Diagnostics = result.Diagnostics.Select(MapDiagnostic).ToArray(),
        };

    private static ConfigDocument MapDocument(GdlConfigModel.ConfigDocument doc) =>
        new()
        {
            Name = doc.Name,
            BasedOnAdr = doc.BasedOnAdr is not null && FSharpOption<string>.get_IsSome(doc.BasedOnAdr) ? doc.BasedOnAdr.Value : null,
            Defaults = new Dictionary<string, string>(doc.Defaults, StringComparer.OrdinalIgnoreCase),
            Sources = doc.Sources.Select(s => new ConfigSourceRow(s.Id, s.Kind, s.Path, s.Slice, s.Line)).ToArray(),
            Contracts = doc.Contracts.Select(c => new ConfigContractRow(c.Id, c.Requires, c.Ensures, c.Line)).ToArray(),
            Facts = doc.Facts.Select(f => new ConfigFactRow(f.Contract, f.VerifiedBy, f.Line)).ToArray(),
        };

    private static GdlDiagnostic MapDiagnostic(GdlConfig.ConfigParseDiagnostic d) =>
        new(d.Code, d.Message, d.Line);
}
