using AIGuiders.Platform.Authoring.Emit;
using AIGuiders.Platform.Authoring.Sat;

namespace AIGuiders.Platform.Authoring.Sat.IdeSession;

public sealed class IdeSessionSatObserver : ISatObserver
{
    public string ObserverId => SatObserverIds.IdeSessionGates;

    public int Priority => 0;

    public bool CanObserve(SatContext context)
    {
        if (context.ParsedGateCatalog is not null)
        {
            return true;
        }

        var path = context.Path;
        return !string.IsNullOrWhiteSpace(path)
            && path.Contains("ide-session", StringComparison.OrdinalIgnoreCase)
            && path.EndsWith(".catalog.gdl", StringComparison.OrdinalIgnoreCase);
    }

    public SatRunResult Observe(SatContext context)
    {
        var catalog = context.ParsedGateCatalog;
        var diagnostics = new List<GdlDiagnostic>();

        if (catalog is null)
        {
            if (string.IsNullOrWhiteSpace(context.Path))
            {
                return SatRunResult.Failed(
                    [new("ide-session.gates.path", "IDE session gate catalog path is missing.")],
                    "ide-session gates: missing catalog path");
            }

            var parsed = IdeSessionCatalogParser.ParseFile(context.Path);
            diagnostics.AddRange(parsed.Diagnostics);
            catalog = parsed.Catalog;
        }

        if (catalog is null || catalog.Gates.Count == 0)
        {
            return SatRunResult.Failed(
                diagnostics,
                "ide-session gates: catalog parse failed");
        }

        foreach (var gate in catalog.Gates)
        {
            if (IdeSessionGateCodeRegistry.IsFuturePort(gate.Code))
            {
                continue;
            }

            if (!IdeSessionGateCodeRegistry.IsKnown(gate.Code))
            {
                diagnostics.Add(new(
                    "ide-session.gates.unknown-code",
                    $"Gate `{gate.GateId}` references unknown implementation `{gate.Code}`.",
                    1));
            }
        }

        if (diagnostics.Count > 0)
        {
            return SatRunResult.Failed(
                diagnostics,
                "ide-session gates: unknown gate implementation symbol(s)");
        }

        return SatRunResult.Ok($"ide-session gates: {catalog.Gates.Count} gate(s) verified");
    }
}
