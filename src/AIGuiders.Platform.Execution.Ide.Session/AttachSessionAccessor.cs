#nullable enable

using AIGuiders.Platform.Modeling.Ide.Session;

namespace AIGuiders.Platform.Execution.Ide.Session;

/// <summary>Session-scoped attach suggestion context (plan §4.4).</summary>
public interface IAttachSessionAccessor
{
    SessionRuntime? TryGet(string? workspaceAnchor);
}

/// <summary>Default accessor — opens federation session cache by workspace anchor path.</summary>
public sealed class FederationAttachSessionAccessor : IAttachSessionAccessor
{
    public static FederationAttachSessionAccessor Instance { get; } = new();

    public SessionRuntime? TryGet(string? workspaceAnchor)
    {
        if (string.IsNullOrWhiteSpace(workspaceAnchor))
            return null;

        try
        {
            return FederationSessionRuntime.Open(workspaceAnchor).Runtime;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
