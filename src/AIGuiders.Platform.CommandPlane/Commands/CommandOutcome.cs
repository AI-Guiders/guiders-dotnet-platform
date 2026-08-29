#nullable enable

namespace AIGuiders.Platform.CommandPlane.Commands;

/// <summary>Command execution result. Buffer edits are synchronous; remote commands extend later.</summary>
public sealed class CommandOutcome
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public EditorBufferOutcome? EditorBuffer { get; init; }

    public static CommandOutcome Ok(EditorBufferOutcome? editor = null) =>
        new() { Success = true, EditorBuffer = editor };

    public static CommandOutcome Fail(string error) =>
        new() { Success = false, Error = error };
}

/// <summary>Editor buffer command result payload.</summary>
public sealed class EditorBufferOutcome
{
    public string? Text { get; init; }
    public int? SelectionStart { get; init; }
    public int? SelectionEnd { get; init; }
    public string? TextMode { get; init; }

    public static EditorBufferOutcome SetTextMode(string mode) =>
        new() { TextMode = mode };
}
