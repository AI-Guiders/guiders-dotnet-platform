namespace AIGuiders.Platform.Execution.Utilities.Adoption.Sources;

/// <summary>Meta-bundle facades — same pattern as <c>CommandSources</c>.</summary>
public static class AdoptionSources
{
    public static IAdoptionSource FromPlanetTree() => new PlanetTreeAdoptionSource();

    public static IAdoptionSource FromCsProjFile(string path, string packageIdPrefix = "AIGuiders.Platform.") =>
        new CsProjFileAdoptionSource(path, packageIdPrefix);

    public static IAdoptionSource FromPackageJsonFile(string path) =>
        new PackageJsonFileAdoptionSource(path);

    private sealed class CsProjFileAdoptionSource(string path, string prefix) : IAdoptionSource
    {
        public AdoptionPartialFacts Read(AdoptionSourceContext context) =>
            CsProjAdoptionReader.ReadFile(path, prefix);
    }

    private sealed class PackageJsonFileAdoptionSource(string path) : IAdoptionSource
    {
        public AdoptionPartialFacts Read(AdoptionSourceContext context) =>
            PackageJsonAdoptionReader.ReadFile(path);
    }
}
