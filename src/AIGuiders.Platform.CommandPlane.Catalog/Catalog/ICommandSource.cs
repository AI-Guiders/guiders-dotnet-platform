using AIGuiders.Platform.IntermediateRepresentation.Command;
#nullable enable

using AIGuiders.Platform.Sources;

namespace AIGuiders.Platform.CommandPlane;

/// <summary>Loads slash command descriptors from a product-owned backend (code, file, DB, HTTP).</summary>
public interface ICommandSource : ISource<IReadOnlyList<CommandDescriptor>>
{
}
