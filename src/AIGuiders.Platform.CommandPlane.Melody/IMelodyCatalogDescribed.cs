#nullable enable

namespace AIGuiders.Platform.CommandPlane.Melody;

/// <summary>Registry command that can project a melody catalog row (GUIDERS-ADR-0015).</summary>
public interface IMelodyCatalogDescribed
{
    MelodyDescriptor ToMelodyDescriptor();
}
