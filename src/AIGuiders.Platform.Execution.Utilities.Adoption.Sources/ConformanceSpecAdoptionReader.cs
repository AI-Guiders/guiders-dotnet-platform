namespace AIGuiders.Platform.Execution.Utilities.Adoption.Sources;

/// <summary>Format reader: embedded conformance *.spec.json → spec tags.</summary>
public static class ConformanceSpecAdoptionReader
{
    public static AdoptionPartialFacts ReadFile(string path)
    {
        var name = Path.GetFileNameWithoutExtension(path);
        if (!name.Contains("spec", StringComparison.OrdinalIgnoreCase))
            return AdoptionPartialFacts.Empty;

        var tag = name.Replace(".spec", "", StringComparison.OrdinalIgnoreCase);
        return new AdoptionPartialFacts([], [new AdoptionSpecTag(tag)]);
    }
}
