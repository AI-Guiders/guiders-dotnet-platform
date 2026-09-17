#nullable enable

using AIGuiders.Platform.Modeling.Ide.Session;
using AIGuiders.Platform.Modeling.LanguageIntelligence.Relations;

namespace AIGuiders.Platform.Execution.LanguageIntelligence.Relations;

/// <summary>ADR-0063 §10.4 — persist typed edge in session graph G from witness.</summary>
public interface IMaterializeRelation
{
    bool TryMaterialize(RelationSpec spec, ResolveCtx ctx, out Relation relation, out string error);
}
