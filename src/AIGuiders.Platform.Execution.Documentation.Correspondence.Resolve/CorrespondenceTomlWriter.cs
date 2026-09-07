#nullable enable

using System.Text;
using AIGuiders.Platform.Execution.Configurations.Workspace;

namespace AIGuiders.Platform.Execution.Documentation.Correspondence;

/// <summary>
/// Line-level merge into <c>.cascade/workspace.toml</c> (forum 003 AddRelated).
/// Comments preserved (never reparse-dump), deterministic block append,
/// dedupe by logical doc path. Honest rejections carry candidate paths.
/// </summary>
public static class CorrespondenceTomlWriter
{
    public sealed record Result(
        bool Ok,
        string? Error,
        string TomlPath,
        string Key,
        string Doc,
        bool Changed,
        string? DocAbs,
        string? DocKind,
        IReadOnlyList<string> Candidates);

    public static Result AddRelatedEntry(string workspaceRoot, string key, string docRaw)
    {
        var tomlPath = WorkspaceSources.CascadeTomlPath(workspaceRoot);
        var k = AIGuiders.Platform.Modeling.Paths.LogicalPath.Normalize(key);
        var d = AIGuiders.Platform.Modeling.Paths.LogicalPath.Normalize(docRaw);
        if (k.Length == 0)
            return new Result(false, "key_required", tomlPath, key, docRaw, false, null, null, []);
        if (k.Contains('"'))
            return new Result(false, "bad_key", tomlPath, k, docRaw, false, null, null, []);

        if (!File.Exists(tomlPath))
            return new Result(false, "no_workspace_toml", tomlPath, k, docRaw, false, null, null, []);

        var resolved = CorrespondenceDocResolve.TryResolve(workspaceRoot, docRaw);
        if (resolved.Abs is null)
            return new Result(false, "doc_not_found", tomlPath, k, resolved.Logical, false, null, null, resolved.Candidates);

        var docLogical = resolved.Logical;

        var text = File.ReadAllText(tomlPath);
        var nl = text.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        var lines = text.Replace("\r\n", "\n").Split('\n').ToList();

        var mapIdx = lines.FindIndex(l => l.TrimStart().StartsWith("[workspace.adr.map]", StringComparison.OrdinalIgnoreCase));
        if (mapIdx < 0)
            return new Result(false, "map_section_missing", tomlPath, k, docLogical, false, resolved.Abs, resolved.Kind, resolved.Candidates);
        var nextSection = -1;
        for (var i = mapIdx + 1; i < lines.Count; i++)
        {
            var t = lines[i].TrimStart();
            if (t.StartsWith('[') && !t.StartsWith("[workspace.adr.map]", StringComparison.OrdinalIgnoreCase))
            {
                nextSection = i;
                break;
            }
        }
        var sectionEnd = nextSection < 0 ? lines.Count : nextSection;

        var keyLine = -1;
        for (var i = mapIdx + 1; i < sectionEnd; i++)
        {
            var t = lines[i].Trim();
            if (t.StartsWith($"\"{k}\"", StringComparison.OrdinalIgnoreCase) && t.Contains('='))
            {
                keyLine = i;
                break;
            }
        }

        if (keyLine < 0)
        {
            var block = new List<string>
            {
                $"\"{k}\" = [",
                $"  \"{docLogical}\",",
                "]",
            };
            var insertAt = sectionEnd;
            if (insertAt > mapIdx + 1 && lines[insertAt - 1].Trim().Length > 0)
                block.Insert(0, string.Empty);
            lines.InsertRange(insertAt, block);
            Write(lines, nl, tomlPath);
            return new Result(true, null, tomlPath, k, docLogical, true, resolved.Abs, resolved.Kind, resolved.Candidates);
        }

        var arrEnd = keyLine;
        while (arrEnd < sectionEnd && !lines[arrEnd].Trim().Equals("]", StringComparison.Ordinal))
            arrEnd++;
        if (arrEnd >= sectionEnd)
            return new Result(false, "malformed_array", tomlPath, k, docLogical, false, resolved.Abs, resolved.Kind, resolved.Candidates);

        for (var i = keyLine + 1; i < arrEnd; i++)
        {
            var t = lines[i].Trim().TrimEnd(',');
            if (t.StartsWith('"') && t.Trim('"').Equals(docLogical, StringComparison.OrdinalIgnoreCase))
                return new Result(true, null, tomlPath, k, docLogical, false, resolved.Abs, resolved.Kind, resolved.Candidates);
        }

        lines.Insert(arrEnd, $"  \"{docLogical}\",");
        Write(lines, nl, tomlPath);
        return new Result(true, null, tomlPath, k, docLogical, true, resolved.Abs, resolved.Kind, resolved.Candidates);
    }

    static void Write(List<string> lines, string nl, string path)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < lines.Count; i++)
        {
            sb.Append(lines[i]);
            if (i < lines.Count - 1)
                sb.Append(nl);
        }
        File.WriteAllText(path, sb.ToString());
    }
}
