using AIGuiders.Platform.Authoring.Emit;

namespace AIGuiders.Platform.Authoring.Sat;

public sealed class AdrFactsSatObserver : ISatObserver
{
    public string ObserverId => SatObserverIds.AdrFacts;

    public int Priority => 100;

    public bool CanObserve(SatContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!string.IsNullOrWhiteSpace(context.AdrId)
            || !string.IsNullOrWhiteSpace(context.FactsPath))
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(context.Path)
            || !context.Path.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!File.Exists(context.Path))
        {
            return false;
        }

        var markdown = File.ReadAllText(context.Path);
        return AdrFactsParser.ContainsFactsBlock(markdown);
    }

    public SatRunResult Observe(SatContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var workspaceRoot = AdrFactsLocator.ResolveWorkspaceRoot(context);
        var sourcePath = ResolveSourcePath(context, workspaceRoot);
        if (sourcePath is null)
        {
            return SatRunResult.Failed(
                [new GdlDiagnostic("sat.adr.missing", "ADR facts source path could not be resolved.")],
                "adr.facts: failed — source not found");
        }

        if (!File.Exists(sourcePath))
        {
            return SatRunResult.Failed(
                [new GdlDiagnostic("sat.adr.missing", $"ADR file not found: {sourcePath}")],
                "adr.facts: failed — file missing");
        }

        var facts = context.ParsedFacts
            ?? AdrFactsParser.TryParse(sourcePath, File.ReadAllText(sourcePath));

        if (facts is null)
        {
            return SatRunResult.Failed(
                [new GdlDiagnostic("sat.adr.facts", $"No facts block found in `{sourcePath}`.")],
                "adr.facts: failed — facts block missing");
        }

        var diagnostics = new List<GdlDiagnostic>();
        var missingGolden = new List<string>();
        foreach (var goldenId in facts.GoldenIds)
        {
            if (!GoldenEvidence.Exists(workspaceRoot, goldenId))
            {
                missingGolden.Add(goldenId);
                diagnostics.Add(new GdlDiagnostic(
                    "sat.golden.missing",
                    $"Golden session `{goldenId}` has no test evidence in workspace `{workspaceRoot}`."));
            }
        }

        var missingHoare = new List<string>();
        var hoareIds = facts.HoareObligations.Select(static o => o.Id).ToArray();
        foreach (var hoareId in hoareIds)
        {
            if (!HoareObligationRegistry.IsRegistered(hoareId))
            {
                missingHoare.Add(hoareId);
                diagnostics.Add(new GdlDiagnostic(
                    "sat.hoare.unregistered",
                    $"Hoare obligation `{hoareId}` is not registered in HoareObligationRegistry."));
            }
        }

        var missingWf = new List<string>();
        foreach (var wfId in facts.WellFormednessIds)
        {
            if (!WellFormednessObligationRegistry.IsRegistered(wfId))
            {
                missingWf.Add(wfId);
                diagnostics.Add(new GdlDiagnostic(
                    "sat.wf.unregistered",
                    $"Well-formedness obligation `{wfId}` is not registered in WellFormednessObligationRegistry."));
            }
        }

        var summary = BuildSummary(facts, hoareIds, missingGolden, missingHoare, missingWf);
        if (diagnostics.Count > 0)
        {
            return SatRunResult.Failed(diagnostics, summary);
        }

        return SatRunResult.Ok(summary);
    }

    private static string? ResolveSourcePath(SatContext context, string workspaceRoot)
    {
        if (!string.IsNullOrWhiteSpace(context.FactsPath))
        {
            return Path.GetFullPath(context.FactsPath);
        }

        if (!string.IsNullOrWhiteSpace(context.Path)
            && File.Exists(context.Path))
        {
            return Path.GetFullPath(context.Path);
        }

        if (!string.IsNullOrWhiteSpace(context.AdrId))
        {
            return AdrFactsLocator.TryResolveAdrPath(workspaceRoot, context.AdrId);
        }

        return null;
    }

    private static string BuildSummary(
        AdrFactsBlock facts,
        IReadOnlyList<string> hoareIds,
        IReadOnlyList<string> missingGolden,
        IReadOnlyList<string> missingHoare,
        IReadOnlyList<string> missingWf)
    {
        var hoareList = hoareIds.Count == 0
            ? "none"
            : string.Join(", ", hoareIds);

        var status = missingGolden.Count == 0 && missingHoare.Count == 0 && missingWf.Count == 0
            ? "ok"
            : "failed";

        return
            $"adr.facts: {status} — {facts.GoldenIds.Count} golden, {facts.HoareObligations.Count} hoare ({hoareList}), {facts.WellFormednessIds.Count} wf, {facts.VerifiedBy.Count} verified_by"
            + (string.IsNullOrWhiteSpace(facts.AdrId) ? string.Empty : $"; adr={facts.AdrId}");
    }

}
