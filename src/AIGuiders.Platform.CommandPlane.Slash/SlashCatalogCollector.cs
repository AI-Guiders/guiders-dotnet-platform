#nullable enable

namespace AIGuiders.Platform.CommandPlane;

/// <summary>Default <see cref="ICatalogVisitor"/> that accumulates descriptors.</summary>
public sealed class SlashCatalogCollector : ICatalogVisitor
{
    readonly List<SlashCommandDescriptor> _descriptors = [];

    public IReadOnlyList<SlashCommandDescriptor> Descriptors => _descriptors;

    public bool Visit(SlashCommandDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        _descriptors.Add(descriptor);
        return true;
    }
}
