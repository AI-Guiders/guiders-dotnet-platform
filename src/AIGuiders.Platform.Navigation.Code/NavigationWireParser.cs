#nullable enable
using System.Text.Json;
using AIGuiders.Platform.Navigation;
using AIGuiders.Platform.Navigation.Policy;

namespace AIGuiders.Platform.Navigation.Code;

public static class NavigationWireParser
{
    public static bool TryParseRelatedWire(
        string json,
        out NavigationAnchor anchor,
        out IReadOnlyList<NavigationRelatedItem> items,
        out string error)
    {
        anchor = new NavigationAnchor("");
        items = [];
        error = "";

        try
        {
            using var doc = JsonDocument.Parse(json);
            return TryParseRelatedElement(doc.RootElement, out anchor, out items, out error);
        }
        catch (JsonException ex)
        {
            error = ex.Message;
            return false;
        }
    }

    public static bool TryParseRelatedElement(
        JsonElement root,
        out NavigationAnchor anchor,
        out IReadOnlyList<NavigationRelatedItem> items,
        out string error)
    {
        anchor = new NavigationAnchor("");
        items = [];
        error = "";

        if (root.TryGetProperty("error", out var errNode)
            && errNode.ValueKind == JsonValueKind.String)
        {
            error = errNode.GetString() ?? "wire error";
            return false;
        }

        var anchorPath = root.TryGetProperty("anchor_path", out var ap)
            ? ap.GetString()
            : root.TryGetProperty("file", out var fp)
                ? fp.GetString()
                : null;

        if (string.IsNullOrWhiteSpace(anchorPath))
        {
            error = "wire missing anchor_path.";
            return false;
        }

        int? line = root.TryGetProperty("line", out var lineNode) && lineNode.TryGetInt32(out var l) ? l : null;
        int? column = root.TryGetProperty("column", out var colNode) && colNode.TryGetInt32(out var c) ? c : null;
        anchor = new NavigationAnchor(anchorPath, line, column);

        if (root.TryGetProperty("items", out var itemsNode) && itemsNode.ValueKind == JsonValueKind.Array)
        {
            items = ParseItems(itemsNode);
            return true;
        }

        if (root.TryGetProperty("nodes", out var nodesNode) && nodesNode.ValueKind == JsonValueKind.Array)
        {
            items = ParseNodesAsItems(nodesNode);
            return true;
        }

        error = "wire missing items[] or nodes[].";
        return false;
    }

    static IReadOnlyList<NavigationRelatedItem> ParseItems(JsonElement array)
    {
        var list = new List<NavigationRelatedItem>();
        foreach (var item in array.EnumerateArray())
        {
            var path = item.TryGetProperty("path", out var p) ? p.GetString() : null;
            if (string.IsNullOrWhiteSpace(path))
                continue;

            var kind = item.TryGetProperty("kind", out var k) ? k.GetString() : "related";
            var rationale = item.TryGetProperty("rationale", out var r) ? r.GetString() : null;
            var rel = item.TryGetProperty("relative_path", out var rp) ? rp.GetString() : null;
            list.Add(new NavigationRelatedItem(path, kind ?? "related", rationale, rel));
        }

        return list;
    }

    static IReadOnlyList<NavigationRelatedItem> ParseNodesAsItems(JsonElement array)
    {
        var list = new List<NavigationRelatedItem>();
        foreach (var node in array.EnumerateArray())
        {
            var path = node.TryGetProperty("path", out var p) ? p.GetString() : null;
            if (string.IsNullOrWhiteSpace(path))
                continue;

            var kind = node.TryGetProperty("kind", out var k) ? k.GetString() : "node";
            if (string.Equals(kind, "anchor", StringComparison.Ordinal))
                continue;

            var rationale = node.TryGetProperty("rationale", out var r) ? r.GetString() : null;
            var rel = node.TryGetProperty("relative_path", out var rp) ? rp.GetString() : null;
            list.Add(new NavigationRelatedItem(path, kind ?? "node", rationale, rel));
        }

        return list;
    }
}
