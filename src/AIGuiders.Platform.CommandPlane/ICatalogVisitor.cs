using AIGuiders.Platform.IntermediateRepresentation.Command;
#nullable enable

namespace AIGuiders.Platform.CommandPlane;

/// <summary>Collects slash catalog rows while visiting a command registry.</summary>
public interface ICatalogVisitor
{
    /// <summary>Returns false to stop visiting remaining commands.</summary>
    bool Visit(CommandDescriptor descriptor);
}
