namespace AIGuiders.Platform.Authoring.Sat;

public static class AdrFactsLocator
{
    public static string? TryResolveAdrPath(string workspaceRoot, string adrId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workspaceRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(adrId);

        var adrDirectory = Path.Combine(workspaceRoot, "docs", "adr");
        if (!Directory.Exists(adrDirectory))
        {
            return null;
        }

        var normalizedId = NormalizeAdrId(adrId);
        var pattern = $"{normalizedId}*.md";
        var matches = Directory
            .EnumerateFiles(adrDirectory, pattern, SearchOption.TopDirectoryOnly)
            .OrderBy(static path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return matches.Length == 0 ? null : matches[0];
    }

    public static string ResolveWorkspaceRoot(SatContext context)
    {
        if (!string.IsNullOrWhiteSpace(context.WorkspaceRoot))
        {
            return Path.GetFullPath(context.WorkspaceRoot);
        }

        if (!string.IsNullOrWhiteSpace(context.Path))
        {
            return FindWorkspaceRoot(Path.GetDirectoryName(Path.GetFullPath(context.Path))!);
        }

        if (!string.IsNullOrWhiteSpace(context.FactsPath))
        {
            return FindWorkspaceRoot(Path.GetDirectoryName(Path.GetFullPath(context.FactsPath))!);
        }

        return FindWorkspaceRoot(Directory.GetCurrentDirectory());
    }

    private static string FindWorkspaceRoot(string startDirectory)
    {
        var current = new DirectoryInfo(startDirectory);
        while (current is not null)
        {
            if (Directory.Exists(Path.Combine(current.FullName, "docs", "adr"))
                || File.Exists(Path.Combine(current.FullName, "AIGuiders.Platform.slnx")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        return startDirectory;
    }

    private static string NormalizeAdrId(string adrId)
    {
        var trimmed = adrId.Trim();
        var fileName = Path.GetFileNameWithoutExtension(trimmed);
        return string.IsNullOrWhiteSpace(fileName) ? trimmed.ToUpperInvariant() : fileName.ToUpperInvariant();
    }
}
