using DotNetWorkspace.Core;
using GdlDotNetPort = AIGuiders.Platform.Modeling.Ide.Session.Ports.DotNet;
using GdlIdentity = AIGuiders.Platform.Modeling.Core.Identity;
using GdlSession = AIGuiders.Platform.Modeling.Ide.Session;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

namespace AIGuiders.Platform.Execution.Ide.Session;

/// <summary>slnx/sln/csproj anchor IO → pure <see cref="GdlDotNetPort.DotNetSlnxGraphPort"/> builders.</summary>
public static class DotNetSlnxGraphSources
{
    public static GdlDotNetPort.DotNetSolutionTopology LoadTopology(string anchorPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(anchorPath);

        var parsed = global::DotNetWorkspace.Core.DotNetWorkspace.Load(anchorPath.Trim());
        var entries = parsed.Projects.ToList();

        var rows = entries
            .Select(entry => new GdlDotNetPort.DotNetProjectTopologyRow(
                entry,
                ListModule.OfSeq(DotNetProjectFileSources.ReadProjectReferences(entry.AbsolutePath)),
                ListModule.OfSeq(DotNetProjectFileSources.ReadSourceFiles(entry.AbsolutePath))))
            .ToList();

        var latest = entries.Count == 0
            ? DateTime.MinValue
            : entries.Max(e => File.GetLastWriteTimeUtc(e.AbsolutePath));

        var fingerprint = $"{parsed.SolutionPath}|projects={entries.Count}|{latest:o}";

        return new GdlDotNetPort.DotNetSolutionTopology(
            parsed.SolutionPath,
            fingerprint,
            ListModule.OfSeq(rows));
    }

    public static GdlSession.SolutionGraph Load(string anchorPath) =>
        GdlDotNetPort.DotNetSlnxGraphPort.buildGraph(LoadTopology(anchorPath));

    public static FSharpMap<string, GdlIdentity.ProjectId> LoadDocumentOwnership(string anchorPath) =>
        GdlDotNetPort.DotNetSlnxGraphPort.buildDocumentOwnership(LoadTopology(anchorPath).Rows);

    public static GdlSession.SolutionSession LoadSession(string anchorPath) =>
        GdlDotNetPort.DotNetSlnxGraphPort.buildSession(LoadTopology(anchorPath));

    public static GdlSession.SessionRuntime LoadRuntime(
        string anchorPath,
        FSharpMap<string, string> sourceOverrides)
    {
        var topology = LoadTopology(anchorPath);
        var ownership = GdlDotNetPort.DotNetSlnxGraphPort.buildDocumentOwnership(topology.Rows);

        var pathContents = ownership.Keys.Select(path =>
        {
            var overrideText = MapModule.TryFind(path, sourceOverrides);
            var text = FSharpOption<string>.get_IsSome(overrideText)
                ? overrideText.Value
                : File.Exists(path)
                    ? File.ReadAllText(path)
                    : string.Empty;

            return Tuple.Create(path, text);
        });

        return GdlDotNetPort.DotNetSlnxGraphPort.buildRuntime(topology, pathContents);
    }
}
