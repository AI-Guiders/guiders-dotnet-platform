#nullable enable

namespace AIGuiders.Platform.Execution.LanguageIntelligence;

public interface IRelationResolver
{
    bool TryResolveRelation(RelationWire wire, out Locus locus, out string error);
}
