using AIGuiders.Platform.Modeling.Ide.Session;
using GdlWorkspacePort = AIGuiders.Platform.Modeling.Ide.Session.Ports.Workspace;
using Microsoft.FSharp.Collections;

namespace AIGuiders.Platform.Execution.Ide.Session;

/// <summary>Workspace document tree scan IO → pure <see cref="GdlWorkspacePort.WorkspaceGraphPort"/>.</summary>
public static class WorkspaceGraphSources
{
    public static WorkspaceGraph Load(string root)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);

        var fullRoot = Path.GetFullPath(root.Trim());
        var snapshots = ScanDocuments(fullRoot).ToList();

        return GdlWorkspacePort.WorkspaceGraphPort.build(
            fullRoot,
            ListModule.OfSeq(snapshots));
    }

    public static string Fingerprint(string root)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);

        var fullRoot = Path.GetFullPath(root.Trim());
        var snapshots = ScanDocuments(fullRoot).ToList();

        return GdlWorkspacePort.WorkspaceGraphPort.fingerprint(
            fullRoot,
            ListModule.OfSeq(snapshots));
    }

    static IEnumerable<GdlWorkspacePort.WorkspaceDocumentSnapshot> ScanDocuments(string root)
    {
        foreach (var path in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
        {
            if (!GdlWorkspacePort.WorkspaceLinks.isDocument(path))
                continue;

            var fullPath = Path.GetFullPath(path);
            yield return new GdlWorkspacePort.WorkspaceDocumentSnapshot(
                fullPath,
                File.ReadAllText(fullPath),
                File.GetLastWriteTimeUtc(fullPath));
        }
    }
}
