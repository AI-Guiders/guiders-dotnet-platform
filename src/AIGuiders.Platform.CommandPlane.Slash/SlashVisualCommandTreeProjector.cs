#nullable enable

namespace AIGuiders.Platform.CommandPlane;

/// <summary>Maps slash completion rows to the shared Visual Command Tree projection (GUIDERS-ADR-0024).</summary>
public static class SlashVisualCommandTreeProjector
{
    public static VisualCommandTreeProjection Project(
        SlashCompletionResult result,
        VisualCommandTreeViewMode viewMode = VisualCommandTreeViewMode.Neighborhood,
        int? optionLimit = null)
    {
        ArgumentNullException.ThrowIfNull(result);

        var guidance = result.Guidance;
        var engageKind = guidance.Phase == InvocationLinePhase.Arg && guidance.ArgMechanic == InvocationArgMechanic.Constructor
            ? VisualCommandTreeEngageKind.Constructor
            : VisualCommandTreeEngageKind.SlashLine;

        var breadcrumbSegments = ParseBreadcrumbSegments(guidance.Breadcrumb);
        var edges = result.Items.Select(ToEdge).ToArray();
        var limit = optionLimit ?? LimitFor(viewMode);
        var next = edges.Take(limit).ToArray();

        return new VisualCommandTreeProjection(
            viewMode,
            engageKind,
            breadcrumbSegments,
            guidance.Breadcrumb,
            ConsumedPrefix: ExtractConsumedPrefix(guidance.Breadcrumb),
            guidance.Placeholder,
            guidance.Hint,
            guidance.Mode,
            next,
            viewMode == VisualCommandTreeViewMode.Full ? edges : null);
    }

    static VisualCommandTreeEdge ToEdge(ArgCompletionItem item)
    {
        var label = item.StepSegment
                    ?? item.PickValue
                    ?? item.Help
                    ?? item.InsertText;
        var kind = item.Kind switch
        {
            ArgCompletionItemKind.ConstructorEntry => VisualCommandTreeNodeKind.ConstructorEntry,
            ArgCompletionItemKind.ConstructorStep => VisualCommandTreeNodeKind.ConstructorStep,
            ArgCompletionItemKind.Picker => VisualCommandTreeNodeKind.Picker,
            _ => VisualCommandTreeNodeKind.Segment,
        };

        return new VisualCommandTreeEdge(
            label,
            item.Help,
            kind,
            item.SlashPath,
            item.PickValue,
            item.InsertText);
    }

    static IReadOnlyList<string> ParseBreadcrumbSegments(string breadcrumb)
    {
        if (string.IsNullOrWhiteSpace(breadcrumb))
        {
            return [];
        }

        var text = breadcrumb.TrimStart('/');
        return text.Split('›', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    static string ExtractConsumedPrefix(string breadcrumb)
    {
        var segments = ParseBreadcrumbSegments(breadcrumb);
        return segments.Count == 0 ? "" : segments[^1];
    }

    static int LimitFor(VisualCommandTreeViewMode viewMode) => viewMode switch
    {
        VisualCommandTreeViewMode.Minimal => VisualCommandTreeProjector.DefaultMinimalLimit,
        VisualCommandTreeViewMode.Neighborhood => VisualCommandTreeProjector.DefaultNeighborhoodLimit,
        VisualCommandTreeViewMode.Full => int.MaxValue,
        _ => VisualCommandTreeProjector.DefaultNeighborhoodLimit,
    };
}
