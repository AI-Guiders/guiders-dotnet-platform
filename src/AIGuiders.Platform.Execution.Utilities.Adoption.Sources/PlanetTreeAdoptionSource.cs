namespace AIGuiders.Platform.Execution.Utilities.Adoption.Sources;

/// <summary>Transport: walk planet tree; dispatch files to format readers (GUIDERS-ADR-0022).</summary>
public sealed class PlanetTreeAdoptionSource : IAdoptionSource
{
    private static readonly HashSet<string> SkipDirs = new(StringComparer.OrdinalIgnoreCase)
    {
        "bin", "obj", "node_modules", ".git", ".cdp",
    };

    public AdoptionPartialFacts Read(AdoptionSourceContext context)
    {
        if (!Directory.Exists(context.PlanetRoot))
            return AdoptionPartialFacts.Empty;

        var pins = new List<AdoptionPin>();
        var specs = new List<AdoptionSpecTag>();

        foreach (var file in EnumerateFiles(context.PlanetRoot))
        {
            var name = Path.GetFileName(file);
            if (name.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
            {
                var partial = CsProjAdoptionReader.ReadFile(file, context.PackageIdPrefix);
                pins.AddRange(partial.Pins);
            }
            else if (name.Equals("package.json", StringComparison.OrdinalIgnoreCase))
            {
                var partial = PackageJsonAdoptionReader.ReadFile(file);
                pins.AddRange(partial.Pins);
            }
            else if (name.EndsWith(".spec.json", StringComparison.OrdinalIgnoreCase))
            {
                var partial = ConformanceSpecAdoptionReader.ReadFile(file);
                specs.AddRange(partial.Specs);
            }
        }

        return new AdoptionPartialFacts(
            DeduplicatePins(pins),
            DeduplicateSpecs(specs));
    }

    private static List<AdoptionPin> DeduplicatePins(List<AdoptionPin> pins) =>
        pins
            .GroupBy(p => p.PackageId, StringComparer.OrdinalIgnoreCase)
            .Select(g =>
            {
                var projectRef = g.FirstOrDefault(p => p.PortKind == AdoptionPortKind.ProjectRef);
                return projectRef ?? g.First();
            })
            .ToList();

    private static List<AdoptionSpecTag> DeduplicateSpecs(List<AdoptionSpecTag> specs) =>
        specs.DistinctBy(s => s.Tag, StringComparer.OrdinalIgnoreCase).ToList();

    private static IEnumerable<string> EnumerateFiles(string root)
    {
        IEnumerable<string> Walk(string dir)
        {
            foreach (var sub in Directory.EnumerateDirectories(dir))
            {
                var name = Path.GetFileName(sub);
                if (SkipDirs.Contains(name))
                    continue;
                foreach (var f in Walk(sub))
                    yield return f;
            }

            foreach (var file in Directory.EnumerateFiles(dir))
                yield return file;
        }

        return Walk(root);
    }
}
