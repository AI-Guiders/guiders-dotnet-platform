#nullable enable
using System.Text.Json;
using System.Text.Json.Serialization;
using AIGuiders.Platform.Navigation;

namespace AIGuiders.Platform.Navigation.Code;

public static class NavigationSceneJson
{
    static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
    };

    public static string ToJson(NavigationScene scene) => JsonSerializer.Serialize(ToDto(scene), Options);

    public static NavigationSceneDto ToDto(NavigationScene scene) =>
        new(
            scene.Schema,
            scene.Mode.ToString().ToLowerInvariant(),
            new NavigationAnchorDto(
                scene.Anchor.Path,
                scene.Anchor.Line,
                scene.Anchor.Column,
                scene.Anchor.SolutionPath),
            scene.Nodes.Select(n => new NavigationNodeDto(
                n.Id,
                n.Path,
                n.Kind,
                n.Rationale,
                n.RelativePath,
                n.Label)).ToList(),
            scene.Edges.Select(e => new NavigationEdgeDto(
                e.FromId,
                e.ToId,
                e.Kind,
                e.RelatedKind)).ToList(),
            new NavigationCapsDto(
                scene.Caps.MaxRelated,
                scene.Caps.MaxNodes,
                scene.Caps.MaxEdges,
                scene.Caps.Preset,
                scene.Caps.KindCaps),
            scene.Summary);

    public sealed record NavigationSceneDto(
        string Schema,
        string Mode,
        NavigationAnchorDto Anchor,
        IReadOnlyList<NavigationNodeDto> Nodes,
        IReadOnlyList<NavigationEdgeDto> Edges,
        NavigationCapsDto Caps,
        string Summary);

    public sealed record NavigationAnchorDto(
        string Path,
        int? Line,
        int? Column,
        string? SolutionPath);

    public sealed record NavigationNodeDto(
        string Id,
        string Path,
        string Kind,
        string? Rationale,
        string? RelativePath,
        string? Label);

    public sealed record NavigationEdgeDto(
        string FromId,
        string ToId,
        string Kind,
        string? RelatedKind);

    public sealed record NavigationCapsDto(
        int MaxRelated,
        int MaxNodes,
        int MaxEdges,
        string? Preset,
        IReadOnlyDictionary<string, int>? KindCaps);
}
