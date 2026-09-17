#nullable enable

using AIGuiders.Platform.Modeling.LanguageIntelligence.Relations;

namespace AIGuiders.Platform.Execution.LanguageIntelligence.Relations;

/// <summary>ADR-0063 §10.4 — ephemeral geometry from <see cref="RelationSpec"/>.</summary>
public interface IResolveRelation
{
    bool TryResolve(RelationSpec spec, ResolveCtx ctx, out Locus locus, out string error);
}
