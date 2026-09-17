#nullable enable

namespace AIGuiders.Platform.Execution.CommandPlane.ArgSuggestions;

/// <summary>Federated arg suggestion query (GUIDERS-ADR-0040).</summary>
public sealed class ArgSuggestionRequest
{
    public required string SuggestionId { get; init; }
    public required string Partial { get; init; }
    public required CatalogRouteEntry Route { get; init; }
    public required string CanonicalPath { get; init; }

    /// <summary>Workspace anchor (slnx/sln path) for session-scoped contextual pickers.</summary>
    public string? WorkspaceAnchor { get; init; }

    public static ArgSuggestionRequest Create(
        string suggestionId,
        string partial,
        CatalogRouteEntry route,
        string canonicalPath,
        string? workspaceAnchor = null) =>
        new()
        {
            SuggestionId = suggestionId,
            Partial = partial,
            Route = route,
            CanonicalPath = canonicalPath,
            WorkspaceAnchor = workspaceAnchor,
        };
}
