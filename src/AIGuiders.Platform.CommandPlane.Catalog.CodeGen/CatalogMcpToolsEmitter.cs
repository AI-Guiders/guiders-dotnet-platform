using System.Text.Json;
using System.Text.Json.Nodes;
using AIGuiders.Platform.Authoring.Command.Catalog;

namespace AIGuiders.Platform.CommandPlane.Catalog.CodeGen;

public static class CatalogMcpToolsEmitter
{
    public static string EmitJson(CatalogDocument document)
    {
        var tools = new JsonArray();
        foreach (var row in document.Mcp.Where(static r => r.Expose.Equals("yes", StringComparison.OrdinalIgnoreCase)))
        {
            var command = document.Commands.FirstOrDefault(c => c.Command.Equals(row.Command, StringComparison.Ordinal));
            var toolName = document.WireCommandId(row.Command);
            var tool = new JsonObject
            {
                ["name"] = toolName,
                ["description"] = command?.Columns.GetValueOrDefault("summary") ?? row.Command,
            };
            tools.Add(tool);
        }

        return tools.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
    }
}
