#nullable enable

using AIGuiders.Platform.Cockpit.Channels.EnvironmentReadiness.DataAcquisition;
using AIGuiders.Platform.Cockpit.Channels.Primitives;

namespace AIGuiders.Platform.Cockpit.Channels.EnvironmentReadiness.ComputingUnits;

/// <summary>CCU: fold env/LSP/dotnet probes into core ER snapshot (W4 quarry).</summary>
public static class EnvironmentReadinessSnapshotUnit
{
    public readonly record struct Input(
        EnvironmentReadinessChannelContext Channel,
        EnvironmentReadinessCSharpProbeOptions CSharp,
        string NotesConfigUnsetHint);

    public static async Task<EnvironmentReadinessSnapshot> BuildCoreAsync(
        Input input,
        CancellationToken cancellationToken = default)
    {
        var ctx = input.Channel;
        var env = EnvironmentReadinessEnvSnapshot.FromProcess(ctx.Settings.AgentNotesConfigPath);
        var agent = EnvironmentReadinessLampRows.BuildAgentRow(ctx.IsMcpStdioHost, ctx.ActiveAiProvider);
        var envRows = EnvironmentReadinessLampRows.BuildEnvProbeRows(
            env, ctx.Settings.AgentNotesConfigPath, input.NotesConfigUnsetHint);
        var lspRows = EnvironmentReadinessLampRows.BuildLspRows(input.CSharp, ctx.Lsp);
        var dotnet = await EnvironmentReadinessLampRows.ProbeDotnetAsync(cancellationToken).ConfigureAwait(false);

        var devDetails = new List<AnnunciatorLampItem>(1 + lspRows.Count + 1) { agent };
        devDetails.AddRange(lspRows);
        devDetails.Add(dotnet);

        var rows = new List<AnnunciatorLampItem>(devDetails.Count + envRows.Count + 2);
        rows.Add(EnvironmentReadinessLampRows.BuildDevToolsSectionRow(devDetails));
        rows.AddRange(devDetails);
        rows.Add(EnvironmentReadinessLampRows.BuildEnvSectionRow(envRows));
        rows.AddRange(envRows);

        return new EnvironmentReadinessSnapshot(rows);
    }

    public static EnvironmentReadinessSnapshot MergeExtension(
        EnvironmentReadinessSnapshot core,
        IReadOnlyList<AnnunciatorLampItem> extensionRows)
    {
        if (extensionRows.Count == 0)
            return core;

        var merged = new List<AnnunciatorLampItem>(core.Rows.Count + extensionRows.Count);
        merged.AddRange(core.Rows);
        merged.AddRange(extensionRows);
        return new EnvironmentReadinessSnapshot(merged);
    }
}
