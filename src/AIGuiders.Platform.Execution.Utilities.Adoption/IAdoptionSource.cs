namespace AIGuiders.Platform.Execution.Utilities.Adoption;

/// <summary>Source adapter: wire format / transport → <see cref="AdoptionPartialFacts"/>.</summary>
public interface IAdoptionSource
{
    AdoptionPartialFacts Read(AdoptionSourceContext context);
}

public sealed record AdoptionSourceContext(
    string PlanetRoot,
    string PackageIdPrefix = "AIGuiders.Platform.");
