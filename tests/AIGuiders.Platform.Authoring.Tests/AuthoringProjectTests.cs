using AIGuiders.Platform.Authoring.Core;
using Xunit;

namespace AIGuiders.Platform.Authoring.Tests;

public sealed class AuthoringProjectTests
{
    [Fact]
    public void OpenSingleFile_resolves_logical_entry_under_workspace()
    {
        var workspace = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        var catalog = Path.Combine(workspace, "Fixtures", "Authoring", "dash.catalog.gdl");

        var result = AuthoringProjectLoader.OpenSingleFile(workspace, catalog);

        Assert.Empty(result.Diagnostics);
        Assert.NotNull(result.Project);
        Assert.Single(result.Project!.Documents);
        Assert.Equal(AuthoringDocumentKind.LogicalFile, result.Project.Documents[0].Ref.Kind);
        Assert.Contains("catalog dash", result.Project.Documents[0].Text!, StringComparison.Ordinal);
        Assert.StartsWith("Fixtures/", result.Project.Entry.Value, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void OpenSingleFile_reports_missing_entry()
    {
        var workspace = Path.GetTempPath();
        var missing = Path.Combine(workspace, "missing.catalog.gdl");

        var result = AuthoringProjectLoader.OpenSingleFile(workspace, missing);

        Assert.Null(result.Project);
        Assert.Contains(
            result.Diagnostics,
            d => d.Code == AuthoringDiagnosticCode.EntryFileNotFound);
    }

    [Fact]
    public void OpenSingleFile_reports_entry_outside_workspace()
    {
        var workspace = Path.Combine(Path.GetTempPath(), "authoring-ws-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workspace);
        var outside = Path.Combine(Path.GetTempPath(), "outside-" + Guid.NewGuid().ToString("N") + ".catalog.gdl");
        File.WriteAllText(outside, "catalog test");

        try
        {
            var result = AuthoringProjectLoader.OpenSingleFile(workspace, outside);

            Assert.Null(result.Project);
            Assert.Contains(
                result.Diagnostics,
                d => d.Code == AuthoringDiagnosticCode.EntryOutsideWorkspace);
        }
        finally
        {
            File.Delete(outside);
            Directory.Delete(workspace, recursive: true);
        }
    }
}
