#nullable enable

using AIGuiders.Platform.Modeling.Ide.Session;

namespace AIGuiders.Platform.Execution.Language.CSharp.Relations;

/// <summary>Roslyn E_dep emitter stub — Execution port until CompilerServices ingest ships (plan §2.3).</summary>
public static class RoslynDependencyRelationEmitStub
{
    public const string ProfileId = "csharp-roslyn";

    public static IReadOnlyList<Relation> EmitStub(SessionRuntime runtime) =>
        Array.Empty<Relation>();
}
