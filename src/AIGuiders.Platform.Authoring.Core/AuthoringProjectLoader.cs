using AIGuiders.Platform.Paths;

namespace AIGuiders.Platform.Authoring.Core;

public static class AuthoringProjectLoader
{
    public static AuthoringProjectLoadResult OpenSingleFile(string workspaceRoot, string entryFilePath)
    {
        var diagnostics = new List<AuthoringDiagnostic>();
        var root = Path.GetFullPath(workspaceRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        var physical = Path.GetFullPath(entryFilePath);

        if (!File.Exists(physical))
        {
            diagnostics.Add(new(
                AuthoringDiagnosticCode.EntryFileNotFound,
                $"Entry file not found: `{physical}`.",
                1));
            return new() { Diagnostics = diagnostics };
        }

        var logical = PathBoundary.ToLogical(root, physical);
        if (logical is null)
        {
            diagnostics.Add(new(
                AuthoringDiagnosticCode.EntryOutsideWorkspace,
                $"Entry `{physical}` is outside workspace root `{root}`.",
                1));
            return new() { Diagnostics = diagnostics };
        }

        var text = File.ReadAllText(physical);
        var document = ResolvedAuthoringDocument.LogicalFile(logical.Value, text, physical);
        var project = new AuthoringProject(root, logical.Value, [document]);
        return new() { Project = project, Diagnostics = diagnostics };
    }
}
