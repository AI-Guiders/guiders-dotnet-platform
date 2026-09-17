#nullable enable

using AIGuiders.Platform.Modeling.Ide.Session;
using AIGuiders.Platform.Modeling.LanguageIntelligence.Relations;

namespace AIGuiders.Platform.Execution.LanguageIntelligence.Relations;

/// <summary>Ship-5 placeholder until language adapters register concrete resolvers.</summary>
public sealed class UnresolvedRelationSeams : IResolveRelation, IMaterializeRelation
{
    public const string NotRegistered = "Relation resolver not registered for this spec case.";

    public bool TryResolve(RelationSpec spec, ResolveCtx ctx, out Locus locus, out string error)
    {
        locus = default!;
        error = NotRegistered;
        return false;
    }

    public bool TryMaterialize(RelationSpec spec, ResolveCtx ctx, out Relation relation, out string error)
    {
        relation = default!;
        error = NotRegistered;
        return false;
    }
}
