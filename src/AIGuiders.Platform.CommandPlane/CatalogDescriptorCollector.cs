using AIGuiders.Platform.IntermediateRepresentation.Command;
#nullable enable

namespace AIGuiders.Platform.CommandPlane;

/// <summary>Default <see cref="ICatalogVisitor"/> that accumulates descriptors.</summary>
public sealed class CatalogDescriptorCollector : ICatalogVisitor
{
    readonly List<CommandDescriptor> _descriptors = [];

    public IReadOnlyList<CommandDescriptor> Descriptors => _descriptors;

    public bool Visit(CommandDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        _descriptors.Add(descriptor);
        return true;
    }
}
