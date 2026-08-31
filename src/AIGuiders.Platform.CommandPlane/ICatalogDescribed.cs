using AIGuiders.Platform.IntermediateRepresentation.Command;
#nullable enable

namespace AIGuiders.Platform.CommandPlane;

/// <summary>Registry command that can project a slash catalog row (GUIDERS-ADR-0010 W2c).</summary>
public interface ICatalogDescribed
{
    CommandDescriptor ToSlashDescriptor();
}
