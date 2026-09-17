#nullable enable

namespace AIGuiders.Platform.Execution.LanguageIntelligence.Relations;

/// <summary>Execution registry for profile-scoped resolve/materialize seams (ADR-0063 R5).</summary>
public static class RelationSeamRegistry
{
    static readonly Dictionary<string, IResolveRelation> ResolveByProfile = new(StringComparer.OrdinalIgnoreCase);
    static readonly Dictionary<string, IMaterializeRelation> MaterializeByProfile = new(StringComparer.OrdinalIgnoreCase);

    public static IResolveRelation DefaultResolve { get; private set; } = new UnresolvedRelationSeams();

    public static IMaterializeRelation DefaultMaterialize { get; private set; } = new UnresolvedRelationSeams();

    public static IReadOnlyCollection<string> RegisteredProfiles => ResolveByProfile.Keys.ToList();

    public static IResolveRelation ResolveFor(string profileId) =>
        ResolveByProfile.TryGetValue(profileId, out var resolve) ? resolve : DefaultResolve;

    public static IMaterializeRelation MaterializeFor(string profileId) =>
        MaterializeByProfile.TryGetValue(profileId, out var materialize) ? materialize : DefaultMaterialize;

    public static void RegisterProfile(
        string profileId,
        IResolveRelation resolve,
        IMaterializeRelation materialize)
    {
        ResolveByProfile[profileId] = resolve;
        MaterializeByProfile[profileId] = materialize;
    }

    public static void ResetForTests()
    {
        ResolveByProfile.Clear();
        MaterializeByProfile.Clear();
        DefaultResolve = new UnresolvedRelationSeams();
        DefaultMaterialize = new UnresolvedRelationSeams();
    }
}
