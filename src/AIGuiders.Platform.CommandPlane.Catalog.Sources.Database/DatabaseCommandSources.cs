using AIGuiders.Platform.IntermediateRepresentation.Command;
#nullable enable
using AIGuiders.Platform.CommandPlane;

namespace AIGuiders.Platform.CommandPlane.Catalog.Sources;

/// <summary>Database-backed <see cref="ICommandSource"/> factories (GUIDERS-ADR-0013).</summary>
public static class DatabaseCommandSources
{
    /// <summary>
    /// Wraps a product-owned query (EF, Dapper, ADO, HTTP gateway) into an <see cref="ICommandSource"/>.
    /// </summary>
    public static ICommandSource From(
        Func<IReadOnlyList<CommandDescriptor>> query,
        string? sourceId = null) =>
        CommandSource.From(query, sourceId ?? "db");
}
