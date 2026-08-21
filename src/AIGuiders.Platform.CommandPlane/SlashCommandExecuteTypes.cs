#nullable enable

namespace AIGuiders.Platform.CommandPlane;

/// <summary>Forge /commands/execute wire (platform SSOT).</summary>
public sealed class SlashCommandExecuteRequest
{
    public string? Path { get; init; }
    public string? CommandId { get; init; }
    public string? Args { get; init; }
    public IReadOnlyDictionary<string, string>? Context { get; init; }
}

public sealed class SlashCommandExecuteResponse
{
    public required string Kind { get; init; }
    public string? RedirectUrl { get; init; }
    public object? Body { get; init; }
    public string? Error { get; init; }
    public string? ConfirmPath { get; init; }
    public string? ConfirmArgs { get; init; }
}
