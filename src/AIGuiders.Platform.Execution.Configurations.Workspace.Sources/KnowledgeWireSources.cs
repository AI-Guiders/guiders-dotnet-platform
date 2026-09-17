using GdlKnowledgeWire = AIGuiders.Platform.Modeling.Configurations.KnowledgeWire;
using Microsoft.FSharp.Core;

namespace AIGuiders.Platform.Execution.Configurations.Workspace;

/// <summary>agent-notes-mcp.toml discovery and read — Execution IO (plan §7).</summary>
public static class KnowledgeWireSources
{
    public static string? TryFindAgentNotesToml(string workspaceRoot)
    {
        if (string.IsNullOrWhiteSpace(workspaceRoot))
            return null;

        var root = Path.GetFullPath(workspaceRoot.Trim());
        var direct = Path.Combine(root, "agent-notes-mcp.toml");
        if (File.Exists(direct))
            return direct;

        try
        {
            return Directory.EnumerateFiles(root, "agent-notes-mcp.toml", SearchOption.AllDirectories).FirstOrDefault();
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }

    public static GdlKnowledgeWire.PersonalRootWire? TryResolvePersonalRoot(string workspaceRoot)
    {
        var tomlPath = TryFindAgentNotesToml(workspaceRoot);
        if (tomlPath is null)
            return null;

        var wire = GdlKnowledgeWire.tryBuildPersonalRootWire(tomlPath, File.ReadLines(tomlPath));
        if (!FSharpOption<GdlKnowledgeWire.PersonalRootWire>.get_IsSome(wire))
            return null;

        return wire.Value;
    }

    public static bool TryReadKnowledgePrimary(string tomlPath, out string? primary)
    {
        var parsed = GdlKnowledgeWire.parseSectionValue(File.ReadLines(tomlPath), "knowledge", "primary");
        if (!FSharpOption<string>.get_IsSome(parsed))
        {
            primary = null;
            return false;
        }

        primary = parsed.Value;
        return !string.IsNullOrWhiteSpace(primary);
    }
}
