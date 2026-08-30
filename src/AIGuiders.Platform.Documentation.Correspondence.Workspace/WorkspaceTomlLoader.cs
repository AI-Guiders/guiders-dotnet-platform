#nullable enable

using Tomlyn;
using Tomlyn.Model;

namespace AIGuiders.Platform.Documentation.Correspondence;

public static class WorkspaceTomlLoader
{
    public static WorkspaceTomlDoc? TryLoad(string tomlPath)
    {
        if (string.IsNullOrWhiteSpace(tomlPath) || !File.Exists(tomlPath))
            return null;

        try
        {
            var model = Toml.ToModel(File.ReadAllText(tomlPath));
            return Parse(model);
        }
        catch
        {
            return null;
        }
    }

    public static string WorkspaceTomlPath(string workspaceRoot) =>
        Path.Combine(workspaceRoot, ".cascade", "workspace.toml");

    static WorkspaceTomlDoc? Parse(TomlTable model)
    {
        if (!model.TryGetValue("workspace", out var workspaceNode) || workspaceNode is not TomlTable workspace)
            return null;

        return new WorkspaceTomlDoc
        {
            Workspace = new WorkspaceSection
            {
                Adr = ParseAdr(workspace),
                Features = ParseFeatures(workspace),
                Correspondence = ParseCorrespondence(workspace)
            }
        };
    }

    static AdrToml? ParseAdr(TomlTable workspace)
    {
        if (!workspace.TryGetValue("adr", out var adrNode) || adrNode is not TomlTable adr)
            return null;

        return new AdrToml
        {
            AutoInclude = ReadString(adr, "auto_include", "autoInclude"),
            MaxRelated = ReadInt(adr, "max_related", "maxRelated"),
            RootDir = ReadString(adr, "root_dir", "rootDir"),
            Map = ReadStringMap(adr, "map")
        };
    }

    static FeaturesToml? ParseFeatures(TomlTable workspace)
    {
        if (!workspace.TryGetValue("features", out var featuresNode) || featuresNode is not TomlTable features)
            return null;

        if (!features.TryGetValue("feature", out var featureNode))
            return new FeaturesToml();

        var list = new List<FeatureToml>();
        switch (featureNode)
        {
            case TomlTableArray array:
                foreach (var row in array)
                    list.Add(ParseFeature(row));
                break;
            case TomlTable single:
                list.Add(ParseFeature(single));
                break;
        }

        return new FeaturesToml { Feature = list };
    }

    static FeatureToml ParseFeature(TomlTable row) => new()
    {
        Id = ReadString(row, "id"),
        Title = ReadString(row, "title"),
        Paths = ReadStringList(row, "paths"),
        Docs = ReadStringList(row, "docs")
    };

    static CorrespondenceToml? ParseCorrespondence(TomlTable workspace)
    {
        if (!workspace.TryGetValue("correspondence", out var corrNode) || corrNode is not TomlTable corr)
            return null;

        if (!corr.TryGetValue("code_anchors", out var anchorsNode)
            && !corr.TryGetValue("codeAnchors", out anchorsNode))
            return new CorrespondenceToml();

        var list = new List<CodeAnchorToml>();
        switch (anchorsNode)
        {
            case TomlTableArray array:
                foreach (var row in array)
                    list.Add(ParseCodeAnchor(row));
                break;
            case TomlTable single:
                list.Add(ParseCodeAnchor(single));
                break;
        }

        return new CorrespondenceToml { CodeAnchors = list };
    }

    static CodeAnchorToml ParseCodeAnchor(TomlTable row) => new()
    {
        Doc = ReadString(row, "doc"),
        File = ReadString(row, "file"),
        Bracket = ReadString(row, "bracket"),
        LineStart = ReadInt(row, "line_start", "lineStart"),
        LineEnd = ReadInt(row, "line_end", "lineEnd"),
        Kind = ReadString(row, "kind"),
        MemberKey = ReadString(row, "member_key", "memberKey")
    };

    static Dictionary<string, object>? ReadStringMap(TomlTable table, string key)
    {
        if (!table.TryGetValue(key, out var node) || node is not TomlTable map)
            return null;

        var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in map)
            dict[pair.Key] = pair.Value ?? "";
        return dict;
    }

    static List<string> ReadStringList(TomlTable table, string key)
    {
        if (!table.TryGetValue(key, out var node))
            return [];

        return node switch
        {
            TomlArray array => array.Select(x => x?.ToString()?.Trim() ?? "")
                .Where(static x => x.Length > 0)
                .ToList(),
            _ when node is not null => [node.ToString()!.Trim()],
            _ => []
        };
    }

    static string? ReadString(TomlTable table, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (table.TryGetValue(key, out var node))
                return node?.ToString()?.Trim();
        }

        return null;
    }

    static int? ReadInt(TomlTable table, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (!table.TryGetValue(key, out var node) || node is null)
                continue;
            if (node is int i) return i;
            if (int.TryParse(node.ToString(), out var parsed)) return parsed;
        }

        return null;
    }
}
