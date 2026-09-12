using AIGuiders.Platform.Modeling.Ide.Session;
using Microsoft.FSharp.Collections;

namespace AIGuiders.Platform.Execution.Ide.Session;

/// <summary>ADR-0004 — load owned source files from disk into session contents (Execution host IO).</summary>
public static class SessionContentsLoader
{
    public static FSharpMap<string, string> LoadFromDisk(SolutionGraph graph)
    {
        ArgumentNullException.ThrowIfNull(graph);

        var pairs =
            graph.FileOwnership.Keys
                .Where(File.Exists)
                .Select(path => Tuple.Create(path, File.ReadAllText(path)));

        return MapModule.OfSeq(pairs);
    }
}
