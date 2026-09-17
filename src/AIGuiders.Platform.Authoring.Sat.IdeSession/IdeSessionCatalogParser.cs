using AIGuiders.Platform.Authoring.Emit;
using AIGuiders.Platform.Authoring.Sat;
using GdlIdeSession = AIGuiders.Platform.Modeling.Gdl.Parse.IdeSession;
using GdlIdeSessionModel = AIGuiders.Platform.Modeling.IdeSession.GateCatalog;
using Microsoft.FSharp.Core;

namespace AIGuiders.Platform.Authoring.Sat.IdeSession;

public sealed class IdeSessionCatalogParseResult
{
    public IdeSessionGateCatalog? Catalog { get; init; }

    public IReadOnlyList<GdlDiagnostic> Diagnostics { get; init; } = [];
}

/// <summary>
/// Thin C# bridge to F# SSOT parser in <c>Modeling.Gdl.Parse.IdeSession</c> (SAT-004).
/// </summary>
public static class IdeSessionCatalogParser
{
    public static IdeSessionCatalogParseResult ParseFile(string path)
    {
        var text = File.ReadAllText(path);
        return Parse(text, path);
    }

    public static IdeSessionCatalogParseResult Parse(string text, string? sourcePath = null)
    {
        var result = GdlIdeSession.IdeSessionCatalogParser.parseText(text, sourcePath ?? "<text>");
        return Map(result);
    }

    private static IdeSessionCatalogParseResult Map(GdlIdeSession.IdeSessionCatalogParseResult result) =>
        new()
        {
            Catalog = FSharpOption<GdlIdeSessionModel.IdeSessionGateCatalog>.get_IsSome(result.Catalog)
                ? MapCatalog(result.Catalog!.Value)
                : null,
            Diagnostics = result.Diagnostics.Select(MapDiagnostic).ToArray(),
        };

    private static IdeSessionGateCatalog MapCatalog(GdlIdeSessionModel.IdeSessionGateCatalog catalog) =>
        new()
        {
            SourcePath = catalog.SourcePath,
            Gates = catalog.Gates
                .Select(g => new IdeSessionGateRow(g.GateId, g.RejectWhen, g.Code))
                .ToArray(),
        };

    private static GdlDiagnostic MapDiagnostic(GdlIdeSession.IdeSessionCatalogParseDiagnostic d) =>
        new(d.Code, d.Message, d.Line);
}
