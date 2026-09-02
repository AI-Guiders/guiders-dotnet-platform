#nullable enable
using AIGuiders.Platform.Execution.Cockpit.Channels.Primitives;
using AIGuiders.Platform.Execution.Cockpit.ComputingUnits;
using AIGuiders.Platform.Modeling.Cockpit.DataBus;

namespace AIGuiders.Platform.Execution.Cockpit.Channels.EnvironmentReadiness;

/// <summary>Stable cell ids for Environment Readiness deck (ADR 0063).</summary>
public static class EnvironmentReadinessCellIds
{
    public const string DevToolsSection = "environment_dev_tools_section";
    public const string Agent = "environment_agent";
    public const string EnvSection = "environment_env_section";
    public const string AgentNotesFile = "environment_agent_notes_file";
    public const string AgentNotesCanonPath = "environment_agent_notes_canon_path";
    public const string NetcoreDbgPath = "environment_netcoredbg_path";
    public const string CSharpLsp = "environment_csharp_lsp";
    public const string MarkdownLsp = "environment_markdown_lsp";
    public const string DotnetSdk = "environment_dotnet_sdk";
}

/// <summary>Headless settings slice for Environment Readiness channel (no UI host).</summary>
public readonly record struct EnvironmentReadinessSettings(string? AgentNotesConfigPath);

/// <summary>Channel input: settings + LSP projection from DataBus (ADR 0099).</summary>
public readonly record struct EnvironmentReadinessChannelContext(
    EnvironmentReadinessSettings Settings,
    string? SolutionPath,
    IdeHostStateChanged Lsp,
    bool IsMcpStdioHost = false,
    string? ActiveAiProvider = null);

/// <summary>Channel output: lamp strip snapshot (ADR 0023).</summary>
public readonly record struct EnvironmentReadinessSnapshot(
    IReadOnlyList<AnnunciatorLampItem> Rows) : ICockpitComputeUnitPayload;
