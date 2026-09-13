using AIGuiders.Platform.Authoring.Emit;

namespace AIGuiders.Platform.Authoring.Sat;

public sealed class ConfigSatObserver : ISatObserver
{
    public string ObserverId => SatObserverIds.ConfigContract;

    public int Priority => 100;

    public bool CanObserve(SatContext context) =>
        !string.IsNullOrWhiteSpace(context.Path)
        && context.Path.EndsWith(".config.gdl", StringComparison.OrdinalIgnoreCase);

    public SatRunResult Observe(SatContext context)
    {
        var path = context.Path!;
        if (!File.Exists(path))
        {
            return SatRunResult.Failed(
                [new GdlDiagnostic("config-file-not-found", $"Config file not found: `{path}`.")]);
        }

        var parse = ConfigGdlParser.ParseFile(path);
        if (parse.Diagnostics.Count > 0)
        {
            return SatRunResult.Failed(parse.Diagnostics, "sat: config parse failed");
        }

        if (parse.Document is null)
        {
            return SatRunResult.Failed(
                [new GdlDiagnostic("config-parse-empty", "Config parse produced no document.")],
                "sat: config parse failed");
        }

        var failures = new List<GdlDiagnostic>();
        var notes = new List<string>();

        foreach (var contract in parse.Document.Contracts)
        {
            EvaluatePredicates(contract.Requires, "requires", contract, context, parse.Document, failures, notes);
            EvaluatePredicates(contract.Ensures, "ensures", contract, context, parse.Document, failures, notes);
        }

        AppendInstallSeedFactStubs(parse.Document, notes);

        if (failures.Count > 0)
        {
            return SatRunResult.Failed(
                failures,
                $"sat: {failures.Count} contract check(s) failed for `{parse.Document.Name}`");
        }

        var summary = notes.Count > 0
            ? $"sat: ok — {parse.Document.Name} ({string.Join("; ", notes.Distinct(StringComparer.OrdinalIgnoreCase))})"
            : $"sat: ok — {parse.Document.Name}";

        return SatRunResult.Ok(summary);
    }

    private static void EvaluatePredicates(
        string raw,
        string role,
        ConfigContractRow contract,
        SatContext context,
        ConfigDocument document,
        IList<GdlDiagnostic> failures,
        IList<string> notes)
    {
        foreach (var predicate in SplitPredicates(raw))
        {
            var evaluation = ConfigContractRegistry.Evaluate(predicate, context, document);
            if (!string.IsNullOrWhiteSpace(evaluation.Note))
            {
                notes.Add(evaluation.Note);
            }

            if (evaluation.Satisfied)
            {
                continue;
            }

            failures.Add(evaluation.Diagnostic ?? new GdlDiagnostic(
                "config-contract-failed",
                $"Contract `{contract.Id}` {role} predicate `{predicate}` failed.",
                contract.Line));
        }
    }

    private static IEnumerable<string> SplitPredicates(string raw) =>
        string.IsNullOrWhiteSpace(raw)
            ? []
            : raw.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

    private static void AppendInstallSeedFactStubs(ConfigDocument document, IList<string> notes)
    {
        foreach (var fact in document.Facts)
        {
            if (!fact.VerifiedBy.StartsWith("install-cdp.personal-seed@", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            notes.Add(
                $"facts stub: `{fact.VerifiedBy}` for `{fact.Contract}` — install evidence not verified in SAT P2");
        }
    }
}
