using AIGuiders.Platform.Modeling.Ide.Session;
using AIGuiders.Platform.Modeling.Ide.Session.Ports.DotNet;

namespace AIGuiders.Platform.Execution.Ide.Session;

/// <summary>Phase 1b host: slnx → <see cref="SolutionGraph"/> + WF validation.</summary>
public static class SolutionSessionHost
{
    public static SolutionSessionOpenResult Open(string anchorPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(anchorPath);

        var session = DotNetSlnxGraphPort.loadSession(anchorPath);
        var validation = GraphValidation.validate(session.Graph);

        return new SolutionSessionOpenResult(session, validation);
    }
}

public sealed record SolutionSessionOpenResult(SolutionSession Session, GraphValidationResult Validation)
{
    public bool IsValid => Validation.IsValid;
}
