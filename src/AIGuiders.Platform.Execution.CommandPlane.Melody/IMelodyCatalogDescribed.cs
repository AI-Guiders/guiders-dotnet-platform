using AIGuiders.Platform.IntermediateRepresentation.Melody;
#nullable enable

namespace AIGuiders.Platform.Execution.CommandPlane.Melody;

/// <summary>Registry command that can project a melody catalog row (GUIDERS-ADR-0015).</summary>
public interface IMelodyCatalogDescribed
{
    MelodyDescriptor ToMelodyDescriptor();
}
