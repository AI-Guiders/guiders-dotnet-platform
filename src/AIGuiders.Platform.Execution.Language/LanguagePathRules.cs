namespace AIGuiders.Platform.Execution.Language;

/// <summary>Extension-based language id resolution per GUIDERS-ADR-0061 §3.</summary>
public static class LanguagePathRules
{
    public static string? ResolveLanguageId(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        var fileName = Path.GetFileName(path);
        var ext = Path.GetExtension(path);

        if (ext.Equals(".sln", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".slnx", StringComparison.OrdinalIgnoreCase))
            return LanguageIds.Csharp;

        if (ext.Equals(".csproj", StringComparison.OrdinalIgnoreCase))
            return LanguageIds.Csharp;

        if (ext.Equals(".fsproj", StringComparison.OrdinalIgnoreCase))
            return LanguageIds.Fsharp;

        if (ext.Equals(".gdlproj", StringComparison.OrdinalIgnoreCase))
            return LanguageIds.Gdl;

        if (fileName.Equals("tsconfig.json", StringComparison.OrdinalIgnoreCase))
            return LanguageIds.Typescript;

        if (fileName.Equals("pyproject.toml", StringComparison.OrdinalIgnoreCase))
            return LanguageIds.Python;

        if (ext.Equals(".fsx", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".fs", StringComparison.OrdinalIgnoreCase))
            return LanguageIds.Fsharp;

        if (ext.Equals(".cs", StringComparison.OrdinalIgnoreCase))
            return LanguageIds.Csharp;

        if (ext.Equals(".ts", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".tsx", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".js", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".jsx", StringComparison.OrdinalIgnoreCase))
            return LanguageIds.Typescript;

        if (ext.Equals(".ps1", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".psm1", StringComparison.OrdinalIgnoreCase))
            return LanguageIds.PowerShell;

        if (ext.Equals(".py", StringComparison.OrdinalIgnoreCase))
            return LanguageIds.Python;

        if (ext.Equals(".pas", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".dpr", StringComparison.OrdinalIgnoreCase))
            return LanguageIds.Delphi;

        if (ext.Equals(".gdl", StringComparison.OrdinalIgnoreCase))
            return LanguageIds.Gdl;

        return null;
    }
}
