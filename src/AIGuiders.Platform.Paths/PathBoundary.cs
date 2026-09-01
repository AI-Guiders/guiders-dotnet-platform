#nullable enable

using TruePath;

namespace AIGuiders.Platform.Paths;

/// <summary>Maps between OS absolute paths and <see cref="LogicalPath"/> at workspace/repo boundaries.</summary>
public static class PathBoundary
{
    public static LogicalPath? ToLogical(string workspaceRoot, string absolutePath)
    {
        if (string.IsNullOrWhiteSpace(workspaceRoot) || string.IsNullOrWhiteSpace(absolutePath))
            return null;

        try
        {
            var root = AbsolutePath.Create(workspaceRoot.Trim());
            var abs = AbsolutePath.Create(absolutePath.Trim());
            if (!abs.StartsWith(root))
                return null;

            var rel = abs.RelativeTo(root);
            return new LogicalPath(rel.ToString().Replace('\\', '/'));
        }
        catch (ArgumentException)
        {
            return ToLogicalFallback(workspaceRoot, absolutePath);
        }
    }

    public static string? ToPhysical(string workspaceRoot, LogicalPath logical)
    {
        if (string.IsNullOrWhiteSpace(workspaceRoot) || logical.IsEmpty)
            return null;

        try
        {
            var root = AbsolutePath.Create(workspaceRoot.Trim());
            var combined = root / logical.Value;
            return combined.ToString();
        }
        catch (ArgumentException)
        {
            return ToPhysicalFallback(workspaceRoot, logical.Value);
        }
    }

    /// <summary>Canonical absolute path for dedupe/compare (IO boundary).</summary>
    public static string? TryCanonicalPhysical(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        try
        {
            return AbsolutePath.Create(Path.GetFullPath(path.Trim())).ToString();
        }
        catch (ArgumentException)
        {
            try
            {
                return Path.GetFullPath(path.Trim());
            }
            catch
            {
                return null;
            }
        }
    }

    static LogicalPath? ToLogicalFallback(string workspaceRoot, string absolutePath)
    {
        try
        {
            var root = Path.GetFullPath(workspaceRoot)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var abs = Path.GetFullPath(absolutePath);
            if (!abs.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                return null;

            return new LogicalPath(abs[root.Length..].TrimStart('\\', '/'));
        }
        catch
        {
            return null;
        }
    }

    static string? ToPhysicalFallback(string workspaceRoot, string logical)
    {
        try
        {
            var segments = logical.Split('/', StringSplitOptions.RemoveEmptyEntries);
            return Path.GetFullPath(Path.Combine([workspaceRoot.Trim(), .. segments]));
        }
        catch
        {
            return null;
        }
    }
}
