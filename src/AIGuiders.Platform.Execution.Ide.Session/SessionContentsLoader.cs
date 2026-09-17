using AIGuiders.Platform.Modeling.Core.Identity;
using AIGuiders.Platform.Modeling.Ide.Session;
using AIGuiders.Platform.Modeling.Ide.Session.Ports.DotNet;
using Microsoft.FSharp.Collections;

namespace AIGuiders.Platform.Execution.Ide.Session;

/// <summary>ADR-0004 — load owned source files from disk into session contents (Execution host IO).</summary>
public static class SessionContentsLoader
{
    public static FSharpMap<string, string> LoadFromDisk(FSharpMap<string, ProjectId> ownership)
    {
        ArgumentNullException.ThrowIfNull(ownership);

        var pairs =
            ownership.Keys
                .Where(File.Exists)
                .Select(path => Tuple.Create(path, File.ReadAllText(path)));

        return MapModule.OfSeq(pairs);
    }
}
