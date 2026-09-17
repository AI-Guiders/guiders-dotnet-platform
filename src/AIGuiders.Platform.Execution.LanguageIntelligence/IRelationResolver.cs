#nullable enable

using AIGuiders.Platform.IntermediateRepresentation.Language;

namespace AIGuiders.Platform.Execution.LanguageIntelligence;

public interface IRelationResolver
{
    bool TryResolveRelation(RelationWire wire, out Locus locus, out string error);
}
