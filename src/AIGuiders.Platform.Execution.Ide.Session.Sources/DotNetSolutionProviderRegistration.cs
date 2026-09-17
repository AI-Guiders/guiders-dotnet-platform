using AIGuiders.Platform.Modeling.Ide.Session;
using AIGuiders.Platform.Modeling.Ide.Session.Ports.DotNet;
using Microsoft.FSharp.Core;

namespace AIGuiders.Platform.Execution.Ide.Session;

/// <summary>ADR-0210 stage 1 — msbuild provider registration @ Execution composition root.</summary>
public static class DotNetSolutionProviderRegistration
{
    public static void Init()
    {
        SolutionProviderRegistry.register(
            Registration.name,
            FSharpFunc<string, ISolutionInfoProvider>.FromConverter(anchor =>
                (ISolutionInfoProvider)new MsBuildSolutionProvider(DotNetSlnxGraphSources.LoadTopology(anchor))));
    }
}
