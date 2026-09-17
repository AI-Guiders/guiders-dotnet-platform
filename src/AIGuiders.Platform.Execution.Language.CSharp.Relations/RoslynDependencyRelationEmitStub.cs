#nullable enable

using AIGuiders.Platform.Execution.LanguageIntelligence;
using AIGuiders.Platform.Execution.LanguageIntelligence.Relations;
using AIGuiders.Platform.Modeling.Ide.Session;

namespace AIGuiders.Platform.Execution.Language.CSharp.Relations;

/// <summary>Roslyn E_dep emitter stub — Execution port until CompilerServices ingest ships (plan §2.3).</summary>
public static class RoslynDependencyRelationEmitStub
{
    public const string ProfileId = "csharp-roslyn";

    static int registered;

    public static void RegisterDefaults()
    {
        if (Interlocked.Exchange(ref registered, 1) == 1)
            return;

        AdapterSlotRegistry.Register(new AdapterSlot(ProfileId, "roslyn"));
        var seams = new UnresolvedRelationSeams();
        RelationSeamRegistry.RegisterProfile(ProfileId, seams, seams);
    }

    public static void ResetForTests() => Interlocked.Exchange(ref registered, 0);

    public static IReadOnlyList<Relation> EmitStub(SessionRuntime runtime)
    {
        RegisterDefaults();
        return Array.Empty<Relation>();
    }
}
