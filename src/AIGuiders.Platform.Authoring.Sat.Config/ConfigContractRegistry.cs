using AIGuiders.Platform.Authoring.Emit;

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
            static (_, _) => new ConfigContractEvaluation(
                true,
                Note: "hot_l0_sections_present pilot stub — L0 section check deferred to P2"));

        Register(
            "primary_is_personal",
            static (context, _) => EvaluatePrimaryIsPersonal(context));
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
