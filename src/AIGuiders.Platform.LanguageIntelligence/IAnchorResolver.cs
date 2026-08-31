#nullable enable

using AIGuiders.Platform.IntermediateRepresentation.Language;

namespace AIGuiders.Platform.LanguageIntelligence;

public interface IAnchorResolver
{
    bool TryResolve(AnchorWire wire, out Locus locus, out string error);
}
