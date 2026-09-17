using GdlConfigModel = AIGuiders.Platform.Modeling.Configurations;
using GdlConfigPredicates = AIGuiders.Platform.Modeling.Configurations.ContractPredicates;
using Microsoft.FSharp.Core;

namespace AIGuiders.Platform.Execution.Configurations.Workspace;

/// <summary>Config SAT predicate IO — loads workspace files then delegates to pure F# predicates.</summary>
public static class ConfigurationContractSources
{
    public static GdlConfigModel.ConfigPredicateResult EvaluateHotL0SectionsPresent(
        string workspaceRoot,
        GdlConfigModel.ConfigDocument document)
    {
        if (string.IsNullOrWhiteSpace(workspaceRoot))
        {
            return Fail("config-missing-workspace", "hot_l0_sections_present requires WorkspaceRoot in SatContext.");
        }

        var wire = KnowledgeWireSources.TryResolvePersonalRoot(workspaceRoot);
        if (wire is null)
        {
            return Fail(
                "config-personal-root-missing",
                $"Could not resolve personal knowledge root from agent-notes-mcp.toml under '{workspaceRoot}'.");
        }

        var hotTemplate = ResolveSourcePath(document, "personal-hot", "{personal}/agent-notes.md");
        var manifestTemplate = ResolveSourcePath(document, "l0-manifest", "{personal}/knowledge/META/memory-architecture-v1.json");
        var notesPath = ExpandPathTemplate(hotTemplate, wire.PersonalRoot);
        var configuredManifestPath = ExpandPathTemplate(manifestTemplate, wire.PersonalRoot);

        if (!File.Exists(notesPath))
        {
            return Fail("config-hot-notes-missing", $"Personal hot file not found: '{notesPath}'.");
        }

        var notesContent = File.ReadAllText(notesPath);
        var manifestOpt = File.Exists(configuredManifestPath)
            ? FSharpOption<string>.Some(File.ReadAllText(configuredManifestPath))
            : FSharpOption<string>.None;

        return GdlConfigPredicates.evaluateHotL0SectionsPresent(
            wire,
            notesContent,
            manifestOpt,
            document);
    }

    static GdlConfigModel.ConfigPredicateResult Fail(string code, string message) =>
        new()
        {
            Satisfied = false,
            Note = "",
            Diagnostic = new GdlConfigModel.ConfigPredicateDiagnostic { Code = code, Message = message },
        };

    static string ExpandPathTemplate(string template, string personalRoot) =>
        template.Replace("{personal}", personalRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

    static string ResolveSourcePath(GdlConfigModel.ConfigDocument document, string sourceId, string fallback)
    {
        var row = document.Sources?.FirstOrDefault(s =>
            string.Equals(s.Id, sourceId, StringComparison.OrdinalIgnoreCase));
        return string.IsNullOrWhiteSpace(row?.Path) ? fallback : row.Path;
    }
}
