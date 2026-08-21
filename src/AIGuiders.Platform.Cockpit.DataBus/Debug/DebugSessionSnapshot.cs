#nullable enable

namespace AIGuiders.Platform.Cockpit.DataBus.Debug;

/// <summary>Breakpoint row in debug session snapshot (headless quarry ADR 0002).</summary>
public readonly record struct DebugBreakpointSnapshot(string File, int Line, string? Condition);

/// <summary>Variable roots for one DAP scope (Locals, Closures, …).</summary>
public readonly record struct DebugVariableRootScope(string ScopeName, IReadOnlyList<DebugVariableRow> Roots);

/// <summary>One variable row; children resolved via VariablesReference in UI adapters.</summary>
public readonly record struct DebugVariableRow(
    string Name,
    string Value,
    string? Type,
    int VariablesReference = 0,
    int? NamedVariables = null,
    int? IndexedVariables = null);

/// <summary>
/// Canonical in-process debug snapshot (ADR 0002/0099): DAP session updates; CCU/UI/MCP read one model.
/// </summary>
public readonly record struct DebugSessionSnapshot(
    bool HasActiveSession,
    bool IsExecutionStopped,
    string? StoppedFile,
    int StoppedLine,
    string? ExceptionText,
    IReadOnlyList<DebugBreakpointSnapshot> Breakpoints,
    IReadOnlyList<(string Name, string? File, int Line)> StackFrames,
    IReadOnlyList<DebugVariableRootScope> VariableRootScopes,
    int VariablesFrameIndex)
{
    public static DebugSessionSnapshot Empty { get; } = new(
        HasActiveSession: false,
        IsExecutionStopped: false,
        StoppedFile: null,
        StoppedLine: 0,
        ExceptionText: null,
        Breakpoints: Array.Empty<DebugBreakpointSnapshot>(),
        StackFrames: Array.Empty<(string Name, string? File, int Line)>(),
        VariableRootScopes: Array.Empty<DebugVariableRootScope>(),
        VariablesFrameIndex: 0);
}
