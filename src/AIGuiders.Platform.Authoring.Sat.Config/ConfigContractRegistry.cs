using AIGuiders.Platform.Authoring.Emit;
using GdlConfigModel = AIGuiders.Platform.Modeling.Config;
using GdlConfigPredicates = AIGuiders.Platform.Modeling.Config.ContractPredicates;
using GdlPredicateResult = AIGuiders.Platform.Modeling.Config.ConfigPredicateResult;

namespace AIGuiders.Platform.Authoring.Sat;

public sealed record ConfigContractEvaluation(
    bool Satisfied,
    string? Note = null,
    GdlDiagnostic? Diagnostic = null);

public static class ConfigContractRegistry
{
    private static readonly Dictionary<string, Func<SatContext, ConfigDocument, ConfigContractEvaluation>> Predicates =
        new(StringComparer.OrdinalIgnoreCase);

    static ConfigContractRegistry()
    {
        RegisterBuiltInStubs();
        RegisterPilotPredicates();
    }

    public static ConfigContractEvaluation Evaluate(string predicateId, SatContext context, ConfigDocument document)
    {
        var id = predicateId.Trim();
        if (id.Length == 0)
        {
            return new ConfigContractEvaluation(
                false,
                Diagnostic: new GdlDiagnostic("config-empty-predicate", "Empty contract predicate id."));
        }

        if (Predicates.TryGetValue(id, out var evaluate))
        {
            return evaluate(context, document);
        }

        return new ConfigContractEvaluation(
            false,
            Diagnostic: new GdlDiagnostic(
                "config-unknown-predicate",
                $"Unknown contract predicate `{id}`."));
    }

    private static void Register(string predicateId, Func<SatContext, ConfigDocument, ConfigContractEvaluation> evaluate) =>
        Predicates[predicateId] = evaluate;

    private static void RegisterBuiltInStubs()
    {
        Register(
            "file_exists",
            static (_, _) => new ConfigContractEvaluation(
                true,
                Note: "file_exists pilot stub"));

        Register(
            "dir_exists",
            static (_, _) => new ConfigContractEvaluation(
                true,
                Note: "dir_exists pilot stub"));
    }

    private static void RegisterPilotPredicates()
    {
        Register(
            "hot_l0_sections_present",
            static (context, document) => EvaluateHotL0SectionsPresent(context, document));

        Register(
            "primary_is_personal",
            static (context, _) => EvaluatePrimaryIsPersonal(context));
    }

    private static ConfigContractEvaluation EvaluateHotL0SectionsPresent(SatContext context, ConfigDocument document)
    {
        var fsharpDoc = MapToFSharpDocument(document);
        var result = GdlConfigPredicates.evaluateHotL0SectionsPresent(context.WorkspaceRoot ?? string.Empty, fsharpDoc);
        return MapPredicateResult(result);
    }

    private static GdlConfigModel.ConfigDocument MapToFSharpDocument(ConfigDocument document)
    {
        var defaults = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in document.Defaults)
        {
            defaults[pair.Key] = pair.Value;
        }

        return new GdlConfigModel.ConfigDocument
        {
            Name = document.Name,
            BasedOnAdr = string.IsNullOrEmpty(document.BasedOnAdr)
                ? Microsoft.FSharp.Core.FSharpOption<string>.None
                : Microsoft.FSharp.Core.FSharpOption<string>.Some(document.BasedOnAdr),
            Defaults = defaults,
            Sources = document.Sources
                .Select(s => new GdlConfigModel.ConfigSourceRow
                {
                    Id = s.Id,
                    Kind = s.Kind,
                    Path = s.Path,
                    Slice = s.Slice,
                    Line = s.Line,
                })
                .ToArray(),
            Contracts = document.Contracts
                .Select(c => new GdlConfigModel.ConfigContractRow
                {
                    Id = c.Id,
                    Requires = c.Requires,
                    Ensures = c.Ensures,
                    Line = c.Line,
                })
                .ToArray(),
            Facts = document.Facts
                .Select(f => new GdlConfigModel.ConfigFactRow
                {
                    Contract = f.Contract,
                    VerifiedBy = f.VerifiedBy,
                    Line = f.Line,
                })
                .ToArray(),
        };
    }

    private static ConfigContractEvaluation MapPredicateResult(GdlPredicateResult result)
    {
        if (result.Satisfied)
        {
            return new ConfigContractEvaluation(true, Note: string.IsNullOrWhiteSpace(result.Note) ? null : result.Note);
        }

        var diagnostic = result.Diagnostic;
        return new ConfigContractEvaluation(
            false,
            Diagnostic: diagnostic is null || string.IsNullOrWhiteSpace(diagnostic.Code)
                ? new GdlDiagnostic("config-contract-failed", "Contract predicate failed.")
                : new GdlDiagnostic(diagnostic.Code, diagnostic.Message));
    }

    private static ConfigContractEvaluation EvaluatePrimaryIsPersonal(SatContext context)
    {
        if (string.IsNullOrWhiteSpace(context.WorkspaceRoot))
        {
            return new ConfigContractEvaluation(
                false,
                Diagnostic: new GdlDiagnostic(
                    "config-missing-workspace",
                    "primary_is_personal requires WorkspaceRoot in SatContext."));
        }

        var tomlPath = FindAgentNotesToml(context.WorkspaceRoot);
        if (tomlPath is null)
        {
            return new ConfigContractEvaluation(
                false,
                Diagnostic: new GdlDiagnostic(
                    "config-notes-toml-missing",
                    $"agent-notes-mcp.toml not found under workspace `{context.WorkspaceRoot}`."));
        }

        if (!TryReadKnowledgePrimary(tomlPath, out var primary))
        {
            return new ConfigContractEvaluation(
                false,
                Diagnostic: new GdlDiagnostic(
                    "config-primary-missing",
                    $"[knowledge].primary not found in `{tomlPath}`."));
        }

        if (!string.Equals(primary, "personal", StringComparison.OrdinalIgnoreCase))
        {
            return new ConfigContractEvaluation(
                false,
                Diagnostic: new GdlDiagnostic(
                    "config-primary-not-personal",
                    $"[knowledge].primary is `{primary}` in `{tomlPath}` (expected `personal`)."));
        }

        return new ConfigContractEvaluation(true, Note: $"primary_is_personal via `{tomlPath}`");
    }

    private static string? FindAgentNotesToml(string workspaceRoot)
    {
        var root = Path.GetFullPath(workspaceRoot);
        var direct = Path.Combine(root, "agent-notes-mcp.toml");
        if (File.Exists(direct))
        {
            return direct;
        }

        try
        {
            foreach (var file in Directory.EnumerateFiles(root, "agent-notes-mcp.toml", SearchOption.AllDirectories))
            {
                return file;
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }

        return null;
    }

    private static bool TryReadKnowledgePrimary(string tomlPath, out string? primary)
    {
        primary = null;
        var inKnowledge = false;

        foreach (var rawLine in File.ReadLines(tomlPath))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            if (line.StartsWith('['))
            {
                inKnowledge = line.Equals("[knowledge]", StringComparison.OrdinalIgnoreCase);
                continue;
            }

            if (!inKnowledge || !line.StartsWith("primary", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var eq = line.IndexOf('=');
            if (eq <= 0)
            {
                continue;
            }

            primary = line[(eq + 1)..].Trim().Trim('"', '\'');
            return primary.Length > 0;
        }

        return false;
    }
}
