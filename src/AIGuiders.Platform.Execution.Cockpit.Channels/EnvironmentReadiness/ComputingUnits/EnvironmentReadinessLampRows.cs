#nullable enable

using System.Diagnostics;
using AIGuiders.Platform.Execution.Cockpit.Channels.EnvironmentReadiness.DataAcquisition;
using AIGuiders.Platform.Execution.Cockpit.Channels.Primitives;
using AIGuiders.Platform.Modeling.Cockpit.DataBus;

namespace AIGuiders.Platform.Execution.Cockpit.Channels.EnvironmentReadiness.ComputingUnits;

/// <summary>Headless ER lamp rows (CIDE quarry W4).</summary>
public static class EnvironmentReadinessLampRows
{
    public static AnnunciatorLampItem BuildSectionRow(
        string cellId,
        string title,
        IReadOnlyList<AnnunciatorLampItem> children,
        string? lampShortLabel = null)
    {
        if (children.Count == 0)
            throw new ArgumentException("Expected at least one detail row.", nameof(children));

        var worst = children[0].Level;
        for (var i = 1; i < children.Count; i++)
            worst = WorstLevel(worst, children[i].Level);

        var level = AggregateSectionLevel(worst);
        var detail = level == AnnunciatorLampLevel.Ok
            ? ""
            : "Есть замечания уровня Caution или выше — см. строки ниже.";
        return new AnnunciatorLampItem(cellId, title, detail, level, LampShortLabel: lampShortLabel ?? "");
    }

    public static AnnunciatorLampItem BuildDevToolsSectionRow(IReadOnlyList<AnnunciatorLampItem> rows) =>
        BuildSectionRow(EnvironmentReadinessCellIds.DevToolsSection, "Dev Tools", rows, "DEV");

    public static AnnunciatorLampItem BuildEnvSectionRow(IReadOnlyList<AnnunciatorLampItem> rows)
    {
        if (rows.Count != 3)
            throw new ArgumentOutOfRangeException(nameof(rows), rows.Count, "Expected Notes, KB, Dbg.");

        var level = AggregateEnvBlockLevel(rows[0].Level, rows[1].Level, rows[2].Level);
        var detail = level == AnnunciatorLampLevel.Ok ? "" : "Есть замечания уровня Caution или выше — см. строки ниже.";
        return new AnnunciatorLampItem(EnvironmentReadinessCellIds.EnvSection, "Переменные окружения", detail, level, LampShortLabel: "ENV");
    }

    public static AnnunciatorLampItem BuildAgentRow(bool isMcpStdioHost, string? activeAiProvider)
    {
        if (isMcpStdioHost)
        {
            return new AnnunciatorLampItem(
                EnvironmentReadinessCellIds.Agent,
                "Агент (MCP)",
                "Запуск с --mcp-stdio: внешний хост вызывает инструменты этой сессии.",
                AnnunciatorLampLevel.Advisory,
                LampShortLabel: "MCP");
        }

        if (string.Equals(activeAiProvider, "CursorACP", StringComparison.Ordinal))
        {
            return new AnnunciatorLampItem(
                EnvironmentReadinessCellIds.Agent,
                "Агент (ACP)",
                "Чат через Cursor ACP: сессия cursor-agent и mcpServers из настроек.",
                AnnunciatorLampLevel.Advisory,
                LampShortLabel: "ACP");
        }

        return new AnnunciatorLampItem(
            EnvironmentReadinessCellIds.Agent,
            "Агент (нет монта)",
            "Нет --mcp-stdio: внешний контур агента не подключён.",
            AnnunciatorLampLevel.Caution,
            LampShortLabel: "Off");
    }

    public static IReadOnlyList<AnnunciatorLampItem> BuildEnvProbeRows(
        EnvironmentReadinessEnvSnapshot env,
        string? notesConfigPath,
        string notesConfigUnsetHint) =>
    [
        BuildAgentNotesFileRow(env.AgentNotesFile),
        BuildAgentNotesConfigRow(env.AgentNotesConfigPath ?? notesConfigPath, notesConfigUnsetHint),
        BuildNetcoreDbgRow(env.NetcoreDbgPath),
    ];

    public static IReadOnlyList<AnnunciatorLampItem> BuildLspRows(
        EnvironmentReadinessCSharpProbeOptions csharp,
        in IdeHostStateChanged lsp) =>
    [
        BuildCSharpRow(csharp, lsp.CSharpLspHostPresent, lsp.CSharpLspProcessActive),
        BuildMarkdownRow(lsp.MarkdownLspHostPresent, lsp.MarkdownLspProcessActive),
    ];

    public static async Task<AnnunciatorLampItem> ProbeDotnetAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            using var p = Process.Start(psi);
            if (p is null)
            {
                return new AnnunciatorLampItem(
                    EnvironmentReadinessCellIds.DotnetSdk,
                    "dotnet (SDK / CLI)",
                    "Не удалось запустить процесс dotnet.",
                    AnnunciatorLampLevel.Critical,
                    LampShortLabel: "NET");
            }

            var stdout = await p.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
            var stderr = await p.StandardError.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
            await p.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
            if (p.ExitCode == 0 && !string.IsNullOrWhiteSpace(stdout))
            {
                return new AnnunciatorLampItem(
                    EnvironmentReadinessCellIds.DotnetSdk,
                    "dotnet (SDK / CLI)",
                    $"Версия: {stdout.Trim()}",
                    AnnunciatorLampLevel.Ok,
                    LampShortLabel: "NET");
            }

            var tail = string.IsNullOrWhiteSpace(stderr) ? $"код выхода {p.ExitCode}" : stderr.Trim();
            return new AnnunciatorLampItem(
                EnvironmentReadinessCellIds.DotnetSdk,
                "dotnet (SDK / CLI)",
                $"dotnet --version не удался ({tail}).",
                AnnunciatorLampLevel.Critical,
                LampShortLabel: "NET");
        }
        catch (Exception ex)
        {
            return new AnnunciatorLampItem(
                EnvironmentReadinessCellIds.DotnetSdk,
                "dotnet (SDK / CLI)",
                $"Не удалось выполнить dotnet --version: {ex.Message}",
                AnnunciatorLampLevel.Critical,
                LampShortLabel: "NET");
        }
    }

    static AnnunciatorLampItem BuildCSharpRow(
        EnvironmentReadinessCSharpProbeOptions csharp,
        bool hostPresent,
        bool processActive)
    {
        if (csharp.InProcessRoslynEnabled)
        {
            return new AnnunciatorLampItem(
                EnvironmentReadinessCellIds.CSharpLsp,
                "C# (in-process)",
                csharp.InProcessRoslynDetail ?? "Диагностика C# через in-process backend; внешний language server не обязателен.",
                AnnunciatorLampLevel.Ok,
                LampShortLabel: "C#");
        }

        if (!hostPresent)
        {
            return new AnnunciatorLampItem(
                EnvironmentReadinessCellIds.CSharpLsp,
                "C# LSP",
                "csharp-ls/OmniSharp/basedpyright не найдены в PATH.",
                AnnunciatorLampLevel.Caution,
                LampShortLabel: "C#");
        }

        return new AnnunciatorLampItem(
            EnvironmentReadinessCellIds.CSharpLsp,
            "C# LSP",
            processActive ? "Внешний LSP: процесс активен." : "Внешний LSP: бинарник на PATH, процесс не активен.",
            processActive ? AnnunciatorLampLevel.Ok : AnnunciatorLampLevel.Caution,
            LampShortLabel: "C#");
    }

    static AnnunciatorLampItem BuildMarkdownRow(bool hostPresent, bool processActive)
    {
        if (!hostPresent)
        {
            return new AnnunciatorLampItem(
                EnvironmentReadinessCellIds.MarkdownLsp,
                "Markdown LSP",
                "marksman не найден в PATH — диагностика MD может быть ограничена.",
                AnnunciatorLampLevel.Advisory,
                LampShortLabel: "MD");
        }

        return new AnnunciatorLampItem(
            EnvironmentReadinessCellIds.MarkdownLsp,
            "Markdown LSP",
            processActive ? "marksman: процесс активен." : "marksman: бинарник на PATH.",
            AnnunciatorLampLevel.Ok,
            LampShortLabel: "MD");
    }

    static AnnunciatorLampItem BuildAgentNotesFileRow(string? raw)
    {
        const string title = WellKnownEnv.AgentNotesFile;
        return EnvironmentReadinessPathAcquisition.ClassifyAgentNotesFilePath(raw) switch
        {
            AgentNotesFilePathKind.Unset => new AnnunciatorLampItem(
                EnvironmentReadinessCellIds.AgentNotesFile, title,
                "Не задана: workspace/.cdp/agent-notes или AGENT_NOTES_FILE для глобального файла.",
                AnnunciatorLampLevel.Ok, LampShortLabel: "Notes"),
            AgentNotesFilePathKind.ParentDirForGlobalFile => new AnnunciatorLampItem(
                EnvironmentReadinessCellIds.AgentNotesFile, title,
                "Задана: каталог для глобального файла заметок существует.",
                AnnunciatorLampLevel.Ok, LampShortLabel: "Notes"),
            AgentNotesFilePathKind.FileExists => new AnnunciatorLampItem(
                EnvironmentReadinessCellIds.AgentNotesFile, title,
                "Задана: глобальный файл заметок существует.",
                AnnunciatorLampLevel.Ok, LampShortLabel: "Notes"),
            AgentNotesFilePathKind.ParentMissing => new AnnunciatorLampItem(
                EnvironmentReadinessCellIds.AgentNotesFile, title,
                "Родительский каталог не найден — проверь AGENT_NOTES_FILE.",
                AnnunciatorLampLevel.Caution, LampShortLabel: "Notes"),
            AgentNotesFilePathKind.InvalidPath => new AnnunciatorLampItem(
                EnvironmentReadinessCellIds.AgentNotesFile, title,
                "Некорректный путь в AGENT_NOTES_FILE.",
                AnnunciatorLampLevel.Critical, LampShortLabel: "Notes"),
            _ => throw new UnreachableException(),
        };
    }

    static AnnunciatorLampItem BuildAgentNotesConfigRow(string? configPath, string unsetHint)
    {
        const string title = "agent-notes config (TOML)";
        return EnvironmentReadinessPathAcquisition.ClassifyAgentNotesConfigPath(configPath) switch
        {
            AgentNotesConfigPathKind.Unset => new AnnunciatorLampItem(
                EnvironmentReadinessCellIds.AgentNotesCanonPath, title, unsetHint,
                AnnunciatorLampLevel.Advisory, LampShortLabel: "KB"),
            AgentNotesConfigPathKind.FileMissing => new AnnunciatorLampItem(
                EnvironmentReadinessCellIds.AgentNotesCanonPath, title,
                "Файл TOML не найден — проверь путь к конфигу.",
                AnnunciatorLampLevel.Caution, LampShortLabel: "KB"),
            AgentNotesConfigPathKind.InvalidPath => new AnnunciatorLampItem(
                EnvironmentReadinessCellIds.AgentNotesCanonPath, title,
                "Некорректный путь в конфиге заметок.",
                AnnunciatorLampLevel.Critical, LampShortLabel: "KB"),
            AgentNotesConfigPathKind.FileExists => new AnnunciatorLampItem(
                EnvironmentReadinessCellIds.AgentNotesCanonPath, title,
                $"TOML найден: {configPath}",
                AnnunciatorLampLevel.Ok, LampShortLabel: "KB"),
            _ => throw new UnreachableException(),
        };
    }

    static AnnunciatorLampItem BuildNetcoreDbgRow(string? raw) =>
        EnvironmentReadinessPathAcquisition.ClassifyNetcoreDbgPath(raw) switch
        {
            NetcoreDbgPathKind.UnsetFoundOnPath => new AnnunciatorLampItem(
                EnvironmentReadinessCellIds.NetcoreDbgPath, WellKnownEnv.NetcoreDbgPath,
                "Не задана: netcoredbg найден в PATH.", AnnunciatorLampLevel.Ok, LampShortLabel: "Dbg"),
            NetcoreDbgPathKind.UnsetNotOnPath => new AnnunciatorLampItem(
                EnvironmentReadinessCellIds.NetcoreDbgPath, WellKnownEnv.NetcoreDbgPath,
                "Не задана: netcoredbg в PATH не найден.", AnnunciatorLampLevel.Advisory, LampShortLabel: "Dbg"),
            NetcoreDbgPathKind.ExplicitResolved => new AnnunciatorLampItem(
                EnvironmentReadinessCellIds.NetcoreDbgPath, WellKnownEnv.NetcoreDbgPath,
                "Исполняемый файл найден.", AnnunciatorLampLevel.Ok, LampShortLabel: "Dbg"),
            NetcoreDbgPathKind.InvalidPath => new AnnunciatorLampItem(
                EnvironmentReadinessCellIds.NetcoreDbgPath, WellKnownEnv.NetcoreDbgPath,
                "Некорректный путь в NETCOREDBG_PATH.", AnnunciatorLampLevel.Critical, LampShortLabel: "Dbg"),
            NetcoreDbgPathKind.ExplicitBareNameNotInPath => new AnnunciatorLampItem(
                EnvironmentReadinessCellIds.NetcoreDbgPath, WellKnownEnv.NetcoreDbgPath,
                "Имя без полного пути: в PATH исполняемый файл не найден.", AnnunciatorLampLevel.Caution, LampShortLabel: "Dbg"),
            NetcoreDbgPathKind.ExplicitFilePathMissing => new AnnunciatorLampItem(
                EnvironmentReadinessCellIds.NetcoreDbgPath, WellKnownEnv.NetcoreDbgPath,
                "Файл по NETCOREDBG_PATH не найден.", AnnunciatorLampLevel.Caution, LampShortLabel: "Dbg"),
            _ => throw new UnreachableException(),
        };

    static AnnunciatorLampLevel AggregateEnvBlockLevel(AnnunciatorLampLevel a, AnnunciatorLampLevel b, AnnunciatorLampLevel c) =>
        AggregateSectionLevel(WorstLevel(WorstLevel(a, b), c));

    static AnnunciatorLampLevel AggregateSectionLevel(AnnunciatorLampLevel worst) =>
        worst is AnnunciatorLampLevel.Caution or AnnunciatorLampLevel.Critical ? worst : AnnunciatorLampLevel.Ok;

    static AnnunciatorLampLevel WorstLevel(AnnunciatorLampLevel a, AnnunciatorLampLevel b) =>
        LevelOrdinal(a) >= LevelOrdinal(b) ? a : b;

    static int LevelOrdinal(AnnunciatorLampLevel l) => l switch
    {
        AnnunciatorLampLevel.Ok => 0,
        AnnunciatorLampLevel.Advisory => 1,
        AnnunciatorLampLevel.Caution => 2,
        AnnunciatorLampLevel.Critical => 3,
        _ => 0,
    };
}
