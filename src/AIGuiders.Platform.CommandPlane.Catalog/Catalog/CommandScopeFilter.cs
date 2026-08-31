#nullable enable

using AIGuiders.Platform.IntermediateRepresentation.Command;

namespace AIGuiders.Platform.CommandPlane;

/// <summary>Catalog visibility by <see cref="CommandDescriptor.Scope"/> (GUIDERS-ADR-0044).</summary>
public static class CommandScopeFilter
{
    /// <summary>Empty descriptor scope = visible in every active scope.</summary>
    public static bool Matches(IReadOnlyList<string> descriptorScope, IReadOnlyList<string> activeScope)
    {
        if (descriptorScope.Count == 0)
        {
            return true;
        }

        if (activeScope.Count == 0)
        {
            return false;
        }

        return descriptorScope.Any(tag =>
            activeScope.Contains(tag, StringComparer.OrdinalIgnoreCase));
    }

    public static bool Matches(CommandDescriptor descriptor, IReadOnlyList<string> activeScope) =>
        Matches(descriptor.Scope, activeScope);

    public static IEnumerable<CommandDescriptor> WhereScope(
        IEnumerable<CommandDescriptor> descriptors,
        IReadOnlyList<string> activeScope) =>
        descriptors.Where(descriptor => Matches(descriptor, activeScope));
}
