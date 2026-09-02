#nullable enable

using AIGuiders.Platform.Execution.Cockpit.DataBus;
using AIGuiders.Platform.Modeling.Cockpit.DataBus;
using Bus = AIGuiders.Platform.Modeling.Cockpit.DataBus;

namespace AIGuiders.Platform.Execution.Cockpit.Channels.IdeHealth.ComputingUnits;

/// <summary>CCU: fold DataBus events into <see cref="IdeHealthInputSnapshot"/> (ADR 0097 quarry).</summary>
public sealed class IdeHealthSnapshotUnit : IDisposable
{
    readonly IdeHealthScopeDecisionUnit _scopeDecision = IdeHealthScopeDecisionUnit.Default;
    readonly IdeHealthBuildTestsUnit _buildTests = IdeHealthBuildTestsUnit.Default;
    readonly IdeHealthDebugSegmentUnit _debugSegment = IdeHealthDebugSegmentUnit.Default;
    readonly IdeHealthGitSegmentUnit _gitSegment = IdeHealthGitSegmentUnit.Default;
    readonly IdeHealthIdeHostUnit _ideHostUnit = IdeHealthIdeHostUnit.Default;
    readonly IDisposable? _buildStateSubscription;
    readonly IDisposable? _testsStateSubscription;
    readonly IDisposable? _debugStateSubscription;
    readonly IDisposable? _gitStateSubscription;
    readonly IDisposable? _ideHostStateSubscription;
    readonly IDisposable? _startupProjectSubscription;
    readonly object _buildSnapshotLock = new();
    Bus.BuildStateSnapshot _buildSnapshot = Bus.BuildStateSnapshot.Empty;
    volatile bool _hasTestsStateFromBus;
    string _latestTestsSummaryFromBus = "";
    int _latestImpactedTestsBadgeFromBus;
    Bus.DebugSessionSnapshot _latestDebugSnapshot = Bus.DebugSessionSnapshot.Empty;
    string _latestGitLine = "Git: —";
    string _latestGitCockpitShort = "GIT · —";
    Bus.IdeHostStateChanged _latestIdeHost = Bus.IdeHostStateChanged.Empty;
    string? _latestStartupProjectPath;
    bool _disposed;

    public IdeHealthSnapshotUnit(IDataBus dataBus)
    {
        _buildStateSubscription = dataBus.Subscribe<Bus.BuildStateChanged>(evt =>
        {
            lock (_buildSnapshotLock)
                _buildSnapshot = BuildStateFold.apply(_buildSnapshot, evt);
        });
        _testsStateSubscription = dataBus.Subscribe<Bus.TestsStateChanged>(evt =>
        {
            _latestTestsSummaryFromBus = evt.Summary ?? "";
            _latestImpactedTestsBadgeFromBus = evt.ImpactedBadge;
            _hasTestsStateFromBus = true;
        });
        _debugStateSubscription = dataBus.Subscribe<Bus.DebugStateChanged>(evt => _latestDebugSnapshot = evt.Snapshot);
        _gitStateSubscription = dataBus.Subscribe<Bus.GitStateChanged>(evt =>
        {
            _latestGitLine = evt.Line;
            _latestGitCockpitShort = evt.CockpitShort;
        });
        _ideHostStateSubscription = dataBus.Subscribe<Bus.IdeHostStateChanged>(evt => _latestIdeHost = evt);
        _startupProjectSubscription = dataBus.Subscribe<Bus.StartupProjectPathChanged>(evt =>
            _latestStartupProjectPath = string.IsNullOrWhiteSpace(evt.ProjectPath) ? null : evt.ProjectPath);
    }

    public IdeHealthInputSnapshot Build(in IdeHealthChannelContext context)
    {
        _ = context;
        ObjectDisposedException.ThrowIf(_disposed, this);

        Bus.BuildStateSnapshot buildState;
        lock (_buildSnapshotLock)
            buildState = _buildSnapshot;

        var testSummary = _hasTestsStateFromBus ? _latestTestsSummaryFromBus : "";
        var impactedTestsBadge = _hasTestsStateFromBus ? _latestImpactedTestsBadgeFromBus : 0;
        var scopeDecision = _scopeDecision.Decide(_latestStartupProjectPath, buildState.IsBuilding, testSummary);
        var buildTests = _buildTests.Compose(scopeDecision, buildState, testSummary, impactedTestsBadge);
        var debug = _debugSegment.Compose(scopeDecision, _latestDebugSnapshot);
        var git = _gitSegment.Compose(_latestGitLine, _latestGitCockpitShort);
        var ideHost = _ideHostUnit.Compose(_latestIdeHost);

        return IdeHealthStrataComposer.Compose(
            new IdeHealthWorkspaceInput(git),
            new IdeHealthSolutionInput(buildTests.Build, buildTests.Tests, debug),
            ideHost);
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        _buildStateSubscription?.Dispose();
        _testsStateSubscription?.Dispose();
        _debugStateSubscription?.Dispose();
        _gitStateSubscription?.Dispose();
        _ideHostStateSubscription?.Dispose();
        _startupProjectSubscription?.Dispose();
    }
}
