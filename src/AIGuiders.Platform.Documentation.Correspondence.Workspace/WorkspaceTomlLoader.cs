#nullable enable

using System.Text.Json;
using Tomlyn;

namespace AIGuiders.Platform.Documentation.Correspondence;

public static class WorkspaceTomlLoader
{
    static readonly TomlSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public static WorkspaceTomlDoc? TryLoad(string tomlPath)
    {
        if (string.IsNullOrWhiteSpace(tomlPath) || !File.Exists(tomlPath))
            return null;

        try
        {
            return TomlSerializer.Deserialize<WorkspaceTomlDoc>(File.ReadAllText(tomlPath), Options);
        }
        catch
        {
            return null;
        }
    }

    public static string WorkspaceTomlPath(string workspaceRoot) =>
        Path.Combine(workspaceRoot, ".cascade", "workspace.toml");
}
