#nullable enable

using AIGuiders.Platform.Modeling.Ide.Session;
using AIGuiders.Platform.Navigation;

namespace AIGuiders.Platform.Navigation.Code;

/// <summary>
/// Execution projection of persisted <see cref="Relation"/> edges into <see cref="NavigationEdge"/>
/// (plan §2.5 — Navigation.Scene is not edge SSOT).
/// </summary>
public static class SceneProjectionBridge
{
    public static NavigationEdge ProjectRelation(string fromId, string toId, Relation relation) =>
        NavigationEdge.FromModel(SceneProjection.toEdge(fromId, toId, relation));

    public static NavigationEdge ProjectRelatedNeighbor(string fromId, string toId, string relatedKind) =>
        NavigationEdge.FromModel(SceneProjection.relatedNeighborEdge(fromId, toId, relatedKind));
}
