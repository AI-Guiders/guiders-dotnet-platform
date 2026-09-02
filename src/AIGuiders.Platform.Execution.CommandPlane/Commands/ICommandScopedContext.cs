#nullable enable

namespace AIGuiders.Platform.Execution.CommandPlane.Commands;

/// <summary>
/// Command context with active catalog scope tags (GUIDERS-ADR-0044).
/// Distinct from <see cref="CommandDescriptor.Surfaces"/> (invoker channel).
/// </summary>
public interface ICommandScopedContext : ICommandContext
{
    /// <summary>Active scope tags for catalog filtering (e.g. dashboard, controlcenter).</summary>
    IReadOnlyList<string> ActiveScope { get; }
}
